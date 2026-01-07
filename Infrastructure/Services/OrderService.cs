
using Application.Assemblers;
using Application.DTOs.Order;
using Application.DTOs.Shipping;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService(PallshoppenDbContext dbContext, OrderAssembler assembler, IInventoryService inventoryService) : IOrderService
{
 
    //Order Tasks
    public async Task<OrderCreatedDto> CreateAsync(CreateOrderRequestDto dto, CancellationToken ct)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Must alteast have one item");

        var orderNumber = await GenerateUniqueOrderNumberAsync(ct);
        var order = await assembler.FromDtoAsync(dto, orderNumber, ct);

        order.SetCartId(dto.CartId);
        var ttl = TimeSpan.FromMinutes(dto.ReservationTtlMinutes <= 0 ? 60 : dto.ReservationTtlMinutes);
        foreach (var i in dto.Items)
        {
            var idempotency = $"{orderNumber}:{i.ProductId}";
            var (ok, err) = await inventoryService.ReserveAsync(i.ProductId, i.Quantity, dto.CartId, idempotency, ttl, ct);
            if (!ok) throw new InvalidOperationException(err ?? "INSUFFICIENT_AVAILABLE");
        }

        dbContext.Orders.Add(order);
                await dbContext.SaveChangesAsync(ct);

        return ToCreatedDto(order);
    }
    public async Task<OrderCreatedDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var o = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? null : ToCreatedDto(o);
    }
    public async Task<OrderCreatedDto?> GetByNumberAsync(string orderNumber, CancellationToken ct)
    {
        var o = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, ct);
        return o is null ? null : ToCreatedDto(o);
    }

    //Stripe payment status updates
    public async Task<bool> MarkPaymentAuthorizedAsync(string orderNumber, string paymentIntentId, string? latestChargeId, string? methodType, decimal amount,string cartId, CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        var (ok, _) = await inventoryService.ConfirmOrderFromCartAsync(cartId, paymentIntentId, ct);

        if (!ok)
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
            foreach (var g in order.OrderItems.GroupBy(i => i.ProductId))
            {
                var qty = g.Sum(x => x.Quantity);
                var affected = await dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [core].[Products] SET OnHand = OnHand - {0}, Reserved = Reserved - {0} " +
                        "WHERE Id = {1} AND OnHand >= {0} AND Reserved >= {0}",
                    [qty, g.Key], ct);

                if (affected == 0)
                {
                    await tx.RollbackAsync(ct);
                    order.Payment.MarkFailed();
                    order.MarkFailed();
                    await dbContext.SaveChangesAsync(ct);
                    return false;
                }
            }
            await tx.CommitAsync(ct);
        }

        order.Payment.MarkAuthorized(paymentIntentId, latestChargeId, amount, methodType, DateTime.UtcNow);
        order.MarkConfirmed();
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> MarkPaymentFailedAsync(string orderNumber, CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return false;

        order.Payment.MarkFailed();
        order.MarkFailed();
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> MarkRefundedAsync(string orderNumber, decimal amount, CancellationToken ct)
    {
        var oder = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (oder is null) return false;

        oder.Payment.MarkRefunded(amount, DateTime.UtcNow);
        oder.MarkRefunded();

        await dbContext.SaveChangesAsync(ct);
        return true;
    }
    
    //helpers.
    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken ct)
    {
        for (var i = 0; i < 3; i++)
        {
            var candidate = GenerateOrderNumber();
            var exists = await dbContext.Orders.AsNoTracking().AnyAsync(o => o.OrderNumber == candidate, ct);
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

    //move to factory later. 
    private static OrderCreatedDto ToCreatedDto(Order o) =>
    new(
        o.Id,
        o.OrderNumber,
        o.CreatedAt,
        o.Currency,
        o.ProductsSubtotal,
        o.ShippingCost,
        o.TaxTotal,
        o.GrandTotal,
        o.OrderStatus,
        o.Payment.Status
    );

    // Shipping selection
    public async Task<bool> SetShippingSelectionAsync(string orderNumber, SetShippingSelectionDto dto, CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
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

        await dbContext.SaveChangesAsync(ct);
        return true;
    }

}
