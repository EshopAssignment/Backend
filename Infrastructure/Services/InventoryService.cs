
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class InventoryService(PallshoppenDbContext dbContext) : IInventoryService
{
    //Certified ClankerMade(ChatGTP5.1) cuz RawSQL makes me want to stab myself in the eye
    public async Task<(bool ok, string? error)> ReserveAsync(int productId, int qty, string cartId, string? idempotencyKey, TimeSpan ttl, CancellationToken ct)
    {
        if (qty <= 0) return (false, "QTY_INVALID");
        var now = DateTimeOffset.UtcNow;

        using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        var affected = await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Reserved = Reserved + {0} WHERE Id = {1} AND (OnHand - Reserved) >= {0}",
            [qty, productId], ct);

        if (affected == 0)
        {
            await tx.RollbackAsync(ct);
            return (false, "INSUFFICIENT_AVAILABLE");
        }

        var res = new StockReservation
        {
            ProductId = productId,
            Quantity = qty,
            CartId = cartId,
            ExpiresAt = now.Add(ttl),
            Status = StockReservationStatus.Active,
            IdempotencyKey = idempotencyKey,
            CreatedAt = now
        };

        dbContext.StockReservations.Add(res);
        await dbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (true, null);
    }
    public async Task ReleaseAsync(long reservationId, CancellationToken ct)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        var res = await dbContext.StockReservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.Status == StockReservationStatus.Active, ct);

        if (res is null)
        {
            await tx.RollbackAsync(ct);
            return;
        }

        var affected = await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Reserved = Reserved - {0} WHERE Id = {1} AND Reserved >= {0}",
            [res.Quantity, res.ProductId], ct);

        if (affected == 0)
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("RESERVED_UNDERFLOW");
        }

        res.Status = StockReservationStatus.Released;
        await dbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
    public async Task<(bool ok, string? error)> ConfirmOrderFromCartAsync(string cartId, string paymentKey, CancellationToken ct)
    {
        var rows = await dbContext.StockReservations
            .Where(r => r.CartId == cartId && r.Status == StockReservationStatus.Active)
            .ToListAsync(ct);

        if (rows.Count == 0) return (false, "NO_ACTIVE_RESERVATIONS");

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        foreach (var g in rows.GroupBy(r => r.ProductId))
        {
            var qty = g.Sum(x => x.Quantity);

            var affected = await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET OnHand = OnHand - {0}, Reserved = Reserved - {0} WHERE Id = {1} AND OnHand >= {0} AND Reserved >= {0}",
                [qty, g.Key], ct);

            if (affected == 0)
            {
                await tx.RollbackAsync(ct);
                return (false, $"INSUFFICIENT_STOCK productId={g.Key}");
            }
        }

        foreach (var r in rows)
        {
            r.Status = StockReservationStatus.Confirmed;
            r.IdempotencyKey ??= paymentKey;
        }

        await dbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (true, null);
    }
    public async Task<int> ReleaseExpiredAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var expired = await dbContext.StockReservations
            .Where(r => r.Status == StockReservationStatus.Active && r.ExpiresAt <= now)
            .ToListAsync(ct);

        if (expired.Count == 0) return 0;

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        foreach (var group in expired.GroupBy(r => r.ProductId))
        {
            var sum = group.Sum(x => x.Quantity);

            var affected = await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET Reserved = Reserved - {0} WHERE Id = {1} AND Reserved >= {0}",
                [sum, group.Key], ct);

            if (affected == 0)
            {
                await tx.RollbackAsync(ct);
                throw new InvalidOperationException($"RESERVED_UNDERFLOW productId={group.Key}");
            }
        }

        foreach (var r in expired) r.Status = StockReservationStatus.Released;

        var count = await dbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return count;
    }

}
