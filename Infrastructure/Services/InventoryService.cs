
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class InventoryService(PallshoppenDbContext dbContext) : IInventoryService
{
    //GPT 5.2 generated.

    //ReserveAsync i now legacy.
    public async Task<(bool ok, string? error)> ReserveAsync(int productId, int qty, string cartId, string? idempotencyKey, TimeSpan ttl, CancellationToken ct)
    {
        if (qty <= 0) return (false, "QTY_INVALID");
        var now = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await dbContext.StockReservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey, ct);

            if (existing is not null)
            {
                if (existing.Status == StockReservationStatus.Active ||
                    existing.Status == StockReservationStatus.Confirmed)
                    return (true, null);

            }
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        var affected = await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE [core].[Products] SET Reserved = Reserved + {0} WHERE Id = {1} AND (OnHand - Reserved) >= {0}",
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

        try
        {
            dbContext.StockReservations.Add(res);
            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return (true, null);
        }
        catch (DbUpdateException ex)
        {

            if (IsUniqueViolation(ex))
            {
                var reverted = await dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [core].[Products] SET Reserved = Reserved - {0} WHERE Id = {1} AND Reserved >= {0}",
                    new object[] { qty, productId }, ct);

                if (reverted == 0)
                {
                    await tx.RollbackAsync(ct);
                    return (false, "RESERVED_UNDERFLOW_ON_DUPLICATE");
                }

                await tx.RollbackAsync(ct);

                var existing = await dbContext.StockReservations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey, ct);

                if (existing is not null &&
                    (existing.Status == StockReservationStatus.Active ||
                     existing.Status == StockReservationStatus.Confirmed))
                    return (true, null);

                return (false, "IDEMPOTENCY_CONFLICT");
            }

            await tx.RollbackAsync(ct);
            throw; 
        }
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
                "UPDATE [core].[Products] SET Reserved = Reserved - {0} WHERE Id = {1} AND Reserved >= {0}",
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
                "UPDATE [core].[Products] " +
                "SET OnHand = OnHand - {0}, Reserved = Reserved - {0} " +
                "WHERE Id = {1} AND OnHand >= {0} AND Reserved >= {0}",
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
            .AsNoTracking()
            .Where(r => r.Status == StockReservationStatus.Active && r.ExpiresAt <= now)
            .Select(r => new { r.Id, r.ProductId, r.Quantity })
            .ToListAsync(ct);

        if (expired.Count == 0) return 0;

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        foreach (var g in expired.GroupBy(x => x.ProductId))
        {
            var sum = g.Sum(x => x.Quantity);

            var affected = await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE [core].[Products] " +
                "SET Reserved = Reserved - {0} " +
                "WHERE Id = {1} AND Reserved >= {0}",
                [sum, g.Key], ct);

            if (affected == 0)
            {
                await tx.RollbackAsync(ct);
                throw new InvalidOperationException($"RESERVED_UNDERFLOW productId={g.Key}");
            }
        }


        var ids = expired.Select(x => x.Id).ToArray();

        var parameters = new List<SqlParameter>(ids.Length);
        var placeholders = new string[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            var p = new SqlParameter($"@p{i}", ids[i]);
            parameters.Add(p);
            placeholders[i] = p.ParameterName;
        }

        var sql =
            $"UPDATE [core].[StockReservations] " +
            $"SET Status = {(int)StockReservationStatus.Released} " +
            $"WHERE Id IN ({string.Join(",", placeholders)})";

        var updated = await dbContext.Database.ExecuteSqlRawAsync(sql, parameters.ToArray(), ct);

        await tx.CommitAsync(ct);

        return updated; 
    }
    static bool IsUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not SqlException sql) return false;
        return sql.Number is 2601 or 2627;
    }
    public async Task<(bool ok, string? error)> SetReservationQtyAsync(
        int productId,
        int desiredQty,
        string cartId,
        TimeSpan ttl,
        CancellationToken ct)
    {
        if (desiredQty < 0) return (false, "QTY_INVALID");

        var now = DateTimeOffset.UtcNow;
        var strat = dbContext.Database.CreateExecutionStrategy();

        return await strat.ExecuteAsync(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var existing = await dbContext.StockReservations
                    .FromSqlRaw(@"
                    SELECT * FROM [core].[StockReservations] WITH (UPDLOCK, HOLDLOCK)
                    WHERE CartId = {0} AND ProductId = {1} AND Status = {2}",
                        cartId, productId, StockReservationStatus.Active)
                    .FirstOrDefaultAsync(ct);

                var currentQty = existing?.Quantity ?? 0;
                if (currentQty == desiredQty)
                {
                    if (existing is not null)
                    {
                        existing.ExpiresAt = now.Add(ttl);
                        await dbContext.SaveChangesAsync(ct);
                        await tx.CommitAsync(ct);
                    }
                    else
                    {
                        await tx.RollbackAsync(ct);
                    }

                    return (true, null);
                }

                var delta = desiredQty - currentQty;

                if (delta > 0)
                {
                    var affected = await dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [core].[Products] SET Reserved = Reserved + {0} " +
                        "WHERE Id = {1} AND (OnHand - Reserved) >= {0}",
                        new object[] { delta, productId }, ct);

                    if (affected == 0)
                    {
                        await tx.RollbackAsync(ct);
                        return (false, "INSUFFICIENT_AVAILABLE");
                    }
                }
                else if (delta < 0)
                {
                    var dec = -delta;
                    var affected = await dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [core].[Products] SET Reserved = Reserved - {0} " +
                        "WHERE Id = {1} AND Reserved >= {0}",
                        new object[] { dec, productId }, ct);

                    if (affected == 0)
                    {
                        await tx.RollbackAsync(ct);
                        return (false, "RESERVED_UNDERFLOW");
                    }
                }

                if (desiredQty == 0)
                {
                    if (existing is not null)
                    {
                        existing.Status = StockReservationStatus.Released;
                        existing.ExpiresAt = now;
                        await dbContext.SaveChangesAsync(ct);
                    }

                    await tx.CommitAsync(ct);
                    return (true, null);
                }

                if (existing is null)
                {
                    dbContext.StockReservations.Add(new StockReservation
                    {
                        ProductId = productId,
                        Quantity = desiredQty,
                        CartId = cartId,
                        ExpiresAt = now.Add(ttl),
                        Status = StockReservationStatus.Active,
                        CreatedAt = now
                    });
                }
                else
                {
                    existing.Quantity = desiredQty;
                    existing.ExpiresAt = now.Add(ttl);
                }

                await dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return (true, null);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<(bool ok, string? error)> VerifyCartReservationAsync(string cartId, IReadOnlyList<(int productId, int qty)> items, CancellationToken ct)
    {
        var active = await dbContext.StockReservations
            .AsNoTracking()
            .Where(r => r.CartId == cartId && r.Status == StockReservationStatus.Active)
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity)})
            .ToListAsync(ct);

        var map = active.ToDictionary(x => x.ProductId, x => x.Qty);

        foreach(var (pid, qty) in items)
        {
            if (qty <= 0) return (false, "QTY_INVALID");
            if(!map.TryGetValue(pid, out var reservedQty)) return(false, $"NOT_RESERVED productId={pid}");
            if (reservedQty < qty) return (false, $"RESERVATION_TOO_LOW productId={pid} reserved={reservedQty} want={qty}");
        }

        return (true, null);
    }
}
