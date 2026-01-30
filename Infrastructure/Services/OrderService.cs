
using System.Runtime.InteropServices;
using Application.Assemblers;
using Application.DTOs.Order;
using Application.DTOs.Shipping;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService(PallshoppenDbContext dbContext, AuthDbContext authContext, OrderAssembler assembler, IInventoryService inventoryService) : IOrderService
{
    private readonly PallshoppenDbContext _db = dbContext;
    private readonly OrderAssembler _assembler = assembler;
    private readonly IInventoryService _inventory = inventoryService;
    private readonly AuthDbContext _authDb = authContext;
    //Order Tasks
    public async Task<OrderCreatedDto> CreateAsync(CreateOrderRequestDto dto, int? userId, CancellationToken ct)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Must at least have one item");

        if (string.IsNullOrWhiteSpace(dto.CartId))
            throw new InvalidOperationException("CartId is required");

        var items = dto.Items.Select(x => (x.ProductId, x.Quantity)).ToList();
        var (ok, err) = await _inventory.VerifyCartReservationAsync(dto.CartId, items, ct);
        if (!ok) throw new InvalidOperationException(err ?? "RESERVATION_MISMATCH");

        var orderNumber = await GenerateUniqueOrderNumberAsync(ct);

        var order = await _assembler.FromDtoAsync(dto, orderNumber, ct);

        order.UserId = userId;

        if (userId is not null)
            await TryAutoFillFromUserAsync(order, userId.Value, ct);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return _assembler.ToCreatedDto(order);
    }
    public async Task<OrderCreatedDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var o = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? null : _assembler.ToCreatedDto(o);
    }
    public async Task<OrderCreatedDto?> GetByNumberAsync(string orderNumber, CancellationToken ct)
    {
        var o = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, ct);
        return o is null ? null : _assembler.ToCreatedDto(o);
    }
    public async Task<OrderDetailsDto?> GetMyOrderByNumberAsync(int userId, string orderNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) return null;

        var o = await _db.Orders
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber && x.UserId == userId, ct);

        return o is null ? null : _assembler.ToDetailsDto(o);
    }
    public async Task<IReadOnlyList<MyOrderListItemDto>> GetMyOrdersAsync(int userId, int skip, int take, CancellationToken ct)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Where(o => o.OrderStatus != OrderStatus.Pending)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(o => new MyOrderListItemDto(
               
                o.CreatedAt,
                o.OrderNumber,
                o.OrderStatus,
                o.GrandTotal,
                TrackingUrl: o.TrackingNumber == null 
                ? null
                : $"https://tracking.postnord.com/?id={Uri.EscapeDataString(o.TrackingNumber)}",
                ReceiptUrl: null
            ))
            .ToListAsync(ct);
    }

    //Stripe payment status updates
    public async Task<bool> MarkPaymentAuthorizedAsync(string orderNumber, string paymentIntentId, string? latestChargeId, string? methodType, decimal amount,string cartId, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        var (ok, _) = await _inventory.ConfirmOrderFromCartAsync(cartId, paymentIntentId, ct);

        if (!ok)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            foreach (var g in order.OrderItems.GroupBy(i => i.ProductId))
            {
                var qty = g.Sum(x => x.Quantity);
                var affected = await _db.Database.ExecuteSqlRawAsync(
                        "UPDATE [core].[Products] SET OnHand = OnHand - {0}, Reserved = Reserved - {0} " +
                        "WHERE Id = {1} AND OnHand >= {0} AND Reserved >= {0}",
                    [qty, g.Key], ct);

                if (affected == 0)
                {
                    await tx.RollbackAsync(ct);
                    order.Payment.MarkFailed();
                    order.MarkFailed();
                    await _db.SaveChangesAsync(ct);
                    return false;
                }
            }
            await tx.CommitAsync(ct);
        }

        order.Payment.MarkAuthorized(paymentIntentId, latestChargeId, amount, methodType, DateTime.UtcNow);
        order.MarkConfirmed();
        await _db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> MarkPaymentFailedAsync(string orderNumber, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        order.Payment.MarkFailed();
        order.MarkFailed();
        await _db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> MarkRefundedAsync(string orderNumber, decimal amount, CancellationToken ct)
    {
        var oder = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (oder is null) return false;

        oder.Payment.MarkRefunded(amount, DateTime.UtcNow);
        oder.MarkRefunded();

        await _db.SaveChangesAsync(ct);
        return true;
    }
    
    //helpers.
    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken ct)
    {
        for (var i = 0; i < 3; i++)
        {
            var candidate = GenerateOrderNumber();
            var exists = await _db.Orders.AsNoTracking().AnyAsync(o => o.OrderNumber == candidate, ct);
            if (!exists) return candidate;
        }
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".ToUpperInvariant();
    }
    private static string GenerateOrderNumber()
    {
        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var rnd = Random.Shared.Next(1000, 9999);
        return $"ORD-{ts}-{rnd}";
    }
    private async Task TryAutoFillFromUserAsync(Order order, int userId, CancellationToken ct)
    {
        var u = await _authDb.Users
            .AsNoTracking()
            .Include(x => x.Profile)
            .ThenInclude(p => p.Addresses!)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (u is null) return;

        var email = u.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return;

        var first = (u.Profile?.FirstName ?? "").Trim();
        var last = (u.Profile?.LastName ?? "").Trim();
        var phone = string.IsNullOrWhiteSpace(u.Profile?.Phone) ? null : u.Profile!.Phone.Trim();


        order.SetCustomerEmail(email);

        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
        {
            order.SetCustomer(first, last, email, phone);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(first)) order.CustomerFirstName = first;
            if (!string.IsNullOrWhiteSpace(last)) order.CustomerLastName = last;
            if (!string.IsNullOrWhiteSpace(phone)) order.CustomerPhoneNumber = phone;

        }

        if (order.ShippingAddress is not null)
            return;

        var profile = u.Profile;
        var addresses = profile?.Addresses?
            .Where(a => a is not null && !a.IsDeleted)
            .ToList();

        if (addresses is null || addresses.Count == 0)
            return;

        var chosen = profile?.DefaultShippingAddressId is int defIdf
            ? addresses.FirstOrDefault(a => a.Id == defIdf) ?? addresses[0]
            : addresses[0];

        var street = (chosen.Street ?? "").Trim();
        var city = (chosen.City ?? "").Trim();
        var postal = (chosen.PostalCode ?? "").Trim().Replace(" ", "");
        var country = string.IsNullOrWhiteSpace(chosen.Country) ? "SE" : chosen.Country.Trim().ToUpperInvariant();

        if (street.Length == 0 || city.Length == 0 || postal.Length == 0)
            return;

        order.SetShippingAddress(new ShippingAddress(
            street: street,
            city: city,
            postalCode: postal,
            country: country
            ));
    }
   
    // Shipping selection
    public async Task<bool> SetShippingSelectionAsync(string orderNumber, SetShippingSelectionDto dto, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

        if (order is null) return false;

        if (order.Payment.Status is Domain.Enums.PaymentStatus.Authorized
            or Domain.Enums.PaymentStatus.Captured
            or Domain.Enums.PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot change shipping after payment authorization.");

        if (dto.ShippingCost < 0) throw new InvalidOperationException("ShippingCost must be >= 0.");

        var carrier = dto.Carrier?.Trim().ToLowerInvariant();
        var method = dto.Method?.Trim().ToLowerInvariant();

        if (carrier != "postnord") throw new InvalidOperationException("Unsupported carrier.");
        if (method != "service_point") throw new InvalidOperationException("Unsupported method.");
        if (string.IsNullOrWhiteSpace(dto.ServicePointId)) throw new InvalidOperationException("ServicePointId is required.");

        order.SetShippingSelection(
            Domain.Enums.ShippingCarrier.PostNord,
            Domain.Enums.ShippingMethod.ServicePoint,
            dto.ShippingCost,
            dto.ServicePointId
        );

        await _db.SaveChangesAsync(ct);
        return true;
    }


    //Checkout Gating Tasks
    public async Task<bool> UpdateCustomerAsync(string orderNumber, UpdateOrderCustomerDto dto, int? userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new InvalidOperationException("OrderNumber is REquired");
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        EnsureCanEdit(order, userId);

        var first = (dto.FirstName ?? "").Trim();
        var last = (dto.LastName ?? "").Trim();
        var email = (dto.Email ?? "").Trim();
        var phone = string.IsNullOrEmpty(dto.Phone) ? null : dto.Phone.Trim();

        if (first.Length == 0) throw new InvalidOperationException("First name is required");
        if (last.Length == 0) throw new InvalidOperationException("Last name is required");
        if (email.Length == 0) throw new InvalidOperationException("Email is required");

        order.SetCustomer(first, last, email, phone);

        await _db.SaveChangesAsync(ct);
        return true;

    }

    public async Task<bool> UpdateShippingAddressAsync(string orderNumber, UpdateOrderShippingAddressDto dto, int? userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new InvalidOperationException("Ordernumber is required");

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        EnsureCanEdit(order, userId);

        var street = (dto.Street ?? "").Trim();
        var city = (dto.City ?? "").Trim();
        var postal = (dto.PostalCode ?? "").Trim();
        var country = string.IsNullOrWhiteSpace(dto.Country) ? "SE" : dto.Country.Trim().ToUpperInvariant();

        if (street.Length == 0) throw new InvalidOperationException("Street is required");
        if (city.Length == 0) throw new InvalidOperationException("City is required");
        if (postal.Length == 0) throw new InvalidOperationException("Postal Code is required");

        postal = postal.Replace(" ", "");

        order.SetShippingAddress(new ShippingAddress(
            street: street,
            city: city,
            postalCode: postal,
            country: country));

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static void EnsureCanEdit(Order order, int? userId)
    {
        if (order.Payment.Status is PaymentStatus.Authorized
            or PaymentStatus.Captured
            or PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot change order after payment is authorized");

        if (order.UserId is not null && userId != order.UserId)
            throw new InvalidOperationException("Not allowed");
    }


}
