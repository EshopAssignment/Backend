using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Application.Interfaces.ACS;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services.Acs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AdminCustomRequestService(
    PallshoppenDbContext dbContext,
    IEmailOutbox emailOutbox,
    IEmailTemplateRenderer templateRenderer,
    ILogger<AdminCustomRequestService> logger) : IAdminCustomRequestService
{
    private readonly PallshoppenDbContext _dbContext = dbContext;
    private readonly IEmailTemplateRenderer _templateRenderer = templateRenderer;
    private readonly IEmailOutbox _emailOutbox = emailOutbox;
    private readonly ILogger<AdminCustomRequestService> _logger = logger;
    public async Task<AdminCustomQuoteDetailsDto> CreateQuoteAsync(int customRequestId, AdminCreateCustomQuoteDto dto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var request = await _dbContext.CustomRequest
            .Include(x => x.Quotes)
            .FirstOrDefaultAsync(x => x.Id == customRequestId, ct)
            ?? throw new KeyNotFoundException($"CustomRequest {customRequestId} finns inte.");

        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Offerten måste innehålla minst en rad.");

        var title = dto.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Titel krävs.");

        var items = dto.Items.Select(BuildItem).ToList();

        var quote = new CustomQuote
        {
            CustomRequestId = request.Id,
            Title = title,
            Currency = "SEK",
            CustomerMessage = string.IsNullOrWhiteSpace(dto.CustomerMessage) ? null : dto.CustomerMessage.Trim(),
            InternalNote = string.IsNullOrWhiteSpace(dto.InternalNote) ? null : dto.InternalNote.Trim(),
            ExpiresAtUtc = dto.ExpiresAtUtc
        };

        quote.ReplaceItems(items);

        _dbContext.CustomQuote.Add(quote);

        if (request.Status == CustomRequestStatus.New)
            request.MarkReviewed();

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created custom quote {QuoteId} for custom request {CustomRequestId}",
            quote.Id,
            request.Id);

        return await MapQuoteDetailsAsync(quote.Id, ct)
            ?? throw new InvalidOperationException("Kunde inte läsa tillbaka skapad offert.");
    }

    public async Task<PagedResult<AdminCustomRequestListItemDto>> GetAllAsync(int page, int pageSize, string? query, string? status, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 200 ? 200 : pageSize;

        var qTerm = query?.Trim();
        var sTerm = status?.Trim();

        var q = _dbContext.CustomRequest
            .AsNoTracking()
            .AsQueryable();

        if(!string.IsNullOrWhiteSpace(qTerm))
        {
            q = q.Where(x => 
            x.Name.Contains(qTerm) ||
            x.Email.Contains(qTerm) ||
            (x.Phone != null && x.Phone.Contains(qTerm)) ||
            x.Message.Contains(qTerm));
        }

        if (!string.IsNullOrWhiteSpace(sTerm) &&
            Enum.TryParse<CustomRequestStatus>(sTerm, true, out var parsedStatus))
        {
            q = q.Where(x => x.Status == parsedStatus);
        }

        q = q.OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminCustomRequestListItemDto(
                Id: x.Id,
                CreatedAtUtc: x.CreatedAtUtc,
                Name: x.Name,
                Email: x.Email,
                Phone: x.Phone,
                Status: x.Status,
                HasAttachment: x.AttatchemntName != null
                ))
            .ToListAsync(ct);

        return new PagedResult<AdminCustomRequestListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AdminCustomRequestDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _dbContext.CustomRequest
            .AsNoTracking()
            .Include(x => x.Quotes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return null;

        var quotes = entity.Quotes
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AdminCustomQuoteListItemDto(
                Id: x.Id,
                CreatedAtUtc: x.CreatedAtUtc,
                Title: x.Title,
                Status: x.Status,
                TotalIncVat: x.TotalIncVat,
                SentAtUtc: x.SentAtUtc,
                ExpiresAtUtc: x.ExpiresAtUtc
            ))
            .ToList();

        return new AdminCustomRequestDetailsDto(
            Id: entity.Id,
            CreatedAtUtc: entity.CreatedAtUtc,
            Name: entity.Name,
            Email: entity.Email,
            Phone: entity.Phone,
            Message: entity.Message,
            Status: entity.Status,
            AttachmentFileName: entity.AttatchemntName,
            AttachmentBlobPath: entity.AttatchemtBlobPath,
            InternalNote: entity.InternalNote,
            Quotes: quotes
        );
    }

    public async Task<bool> SendQuoteAsync(int quoteId, CancellationToken ct)
    {
        var quote = await _dbContext.CustomQuote
            .Include(x => x.CustomRequest)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == quoteId, ct);

        if (quote is null)
            return false;

        if (quote.Items.Count == 0)
            throw new InvalidOperationException("Kan inte skicka en tom offert");

        if (quote.Status != CustomQuoteStatus.Draft)
            throw new InvalidOperationException("Endast Utkast kan skickas");
        
        var request = quote.CustomRequest;

        if(string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("Custom request saknar e-postadress.");

        var customerName = string.IsNullOrWhiteSpace(request.Name) ? "kund" : request.Name.Trim();

        var html = _templateRenderer.RenderCustomQuoteCustomer(
            customerName: customerName,
            quoteTitle: quote.Title,
            currency: quote.Currency,
            totalIncVat: quote.TotalIncVat,
            expiresAtUtc: quote.ExpiresAtUtc,
            customerMessage: quote.CustomerMessage,
            items: quote.Items
                .OrderBy(x => x.Id)
                .Select(x => (
                    x.Description,
                    Qty: x.Quantity,
                    UnitPrice: x.UnitPriceIncVat,
                    LineTotal: x.LineTotalIncVat
                ))
        );

        var correlationId = $"customquote:{quote.Id}:send";

        await _emailOutbox.EnqueueAsync(
            to: request.Email.Trim(),
            subject: $"Din offert från Pallshoppen - {quote.Title}",
            htmlBody: html,
            kind: "custom_quote_customer",
            correlationId: correlationId,
            ct: ct);

        quote.MarkSent();
        request.MarkQuoted();

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Sent CustomQuote {QuoteId} to {Email}",
            quote.Id,
            request.Email);

        return true;
    }



    //helpers 
    private async Task<AdminCustomQuoteDetailsDto?> MapQuoteDetailsAsync(int quoteId, CancellationToken ct)
    {
        var quote = await _dbContext.CustomQuote
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == quoteId, ct);

        if (quote is null)
            return null;

        var items = quote.Items
            .OrderBy(x => x.Id)
            .Select(x => new AdminCustomQuoteItemDto(
                Description: x.Description,
                Quantity: x.Quantity,
                UnitPriceExVat: x.UnitPriceExVat,
                VatRatePercent: x.VatRatePercent,
                UnitVatAmount: x.UnitVatAmount,
                UnitPriceIncVat: x.UnitPriceIncVat,
                LineTotalExVat: x.LineTotalExVat,
                LineTotalVat: x.LineTotalVat,
                LineTotalIncVat: x.LineTotalIncVat
            ))
            .ToList();

        return new AdminCustomQuoteDetailsDto(
            Id: quote.Id,
            CustomRequestId: quote.CustomRequestId,
            Title: quote.Title,
            Currency: quote.Currency,
            CustomerMessage: quote.CustomerMessage,
            InternalNote: quote.InternalNote,
            CreatedAtUtc: quote.CreatedAtUtc,
            SentAtUtc: quote.SentAtUtc,
            ExpiresAtUtc: quote.ExpiresAtUtc,
            Status: quote.Status.ToString(),
            SubtotalExVat: quote.SubtotalExVat,
            VatTotal: quote.VatTotal,
            TotalIncVat: quote.TotalIncVat,
            Items: items
        );
    }

    private static CustomQuoteItem BuildItem(AdminCreateCustomQuoteItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new InvalidOperationException("Beskrivning krävs.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Antal måste vara minst 1.");

        if (dto.UnitPriceExVat < 0)
            throw new InvalidOperationException("Pris måste vara 0 eller mer.");

        if (dto.VatRatePercent is not (6 or 12 or 25))
            throw new InvalidOperationException("Ogiltig momssats.");

        var unitExVat = Math.Round(dto.UnitPriceExVat, 2);
        var unitVat = Math.Round(unitExVat * dto.VatRatePercent / 100m, 2);
        var unitIncVat = Math.Round(unitExVat + unitVat, 2);

        var lineExVat = Math.Round(unitExVat * dto.Quantity, 2);
        var lineVat = Math.Round(unitVat * dto.Quantity, 2);
        var lineIncVat = Math.Round(lineExVat + lineVat, 2);

        return new CustomQuoteItem
        {
            Description = dto.Description.Trim(),
            Quantity = dto.Quantity,
            VatRatePercent = dto.VatRatePercent,
            UnitPriceExVat = unitExVat,
            UnitVatAmount = unitVat,
            UnitPriceIncVat = unitIncVat,
            LineTotalExVat = lineExVat,
            LineTotalVat = lineVat,
            LineTotalIncVat = lineIncVat
        };
    }
}
