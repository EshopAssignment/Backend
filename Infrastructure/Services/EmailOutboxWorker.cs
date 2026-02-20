using Application.Interfaces;
using Domain.Entities.Mail;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class EmailOutboxWorker(IServiceProvider sp, ILogger<EmailOutboxWorker> logger) : BackgroundService
{
    private const int BatchSize = 5;
    private static readonly TimeSpan LoopDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken st)
    {
        while(!st.IsCancellationRequested)
        {
            try
            {
                await ProcessOnce(st);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error processing email outbox"); 
            }

            await Task.Delay(LoopDelay, st);
        }
    }

    private async Task ProcessOnce(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var now = DateTime.UtcNow;

        var claimed = await ClaimBatchAsync(db, now, BatchSize, ct);
        if (claimed.Count == 0) return;

        foreach(var msg in claimed)
        {
            try
            {
                await sender.SendAsync(msg.To, msg.Subject, msg.HtmlBody, ct);

                msg.Status = EmailOutboxStatus.Sent;
                msg.SentAt = DateTime.UtcNow;
                msg.LastError = null;

                logger.LogInformation("Sent email to {To} with subject {Subject}", msg.Id, msg.To, msg.Kind);
            } 
            catch(Exception ex)
            {
                msg.Attempts += 1;
                msg.LastAttempt = DateTime.UtcNow;

                var delay = TimeSpan.FromSeconds(Math.Min(300, 2 * Math.Pow(2, Math.Min(2, msg.Attempts))));
                msg.NextAttempt = DateTime.UtcNow.Add(delay);

                if(msg.Attempts >= 5)
                {
                    msg.Status = EmailOutboxStatus.Failed;

                }
                else
                {
                msg.Status = EmailOutboxStatus.Processing;
                }

                msg.LastError = Truncate(ex.ToString(), 4000);

                logger.LogWarning(ex,
                "Email failed. OutboxId={Id} Attempt={Attempt} Next={Next} Status={Status}",
                msg.Id, msg.Attempts, msg.NextAttempt, msg.Status);
            }

            await db.SaveChangesAsync(ct);
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private static async Task<List<EmailOutboxMessage>> ClaimBatchAsync(PallshoppenDbContext db, DateTimeOffset now, int take, CancellationToken ct)
    {
        var sql = @"
        ;WITH cte AS (
            SELECT TOP (@take) *
            FROM [core].[EmailOutbox] WITH (READPAST, UPDLOCK, ROWLOCK)
            WHERE [Status] = @pending
              AND [NextAttemptAtUtc] <= @now
            ORDER BY [NextAttemptAtUtc] ASC, [Id] ASC
        )
        UPDATE cte
        SET [Status] = @processing
        OUTPUT inserted.*;";

        var pending = (int)EmailOutboxStatus.Pending;
        var processing = (int)EmailOutboxStatus.Processing;

        var rows = await db.EmailOutBox
            .FromSqlRaw(sql,
                new SqlParameter("@take", take),
                new SqlParameter("@pending", pending),
                new SqlParameter("@processing", processing),
                new SqlParameter("@now", now))
            .AsTracking()
            .ToListAsync(ct);

        return rows;
    }
}
