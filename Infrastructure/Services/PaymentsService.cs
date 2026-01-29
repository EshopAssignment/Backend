using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;

public class PaymentsService(PallshoppenDbContext dbContext, StripeClient stripeClient)
{
    private readonly PallshoppenDbContext _db = dbContext;
    private readonly PaymentIntentService _pi = new(stripeClient);

    private static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    public async Task<string> CreateOrGetClientSecretAsync(string orderNumber, string cartId, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct)
                   ?? throw new InvalidOperationException("Order not found");

        if (!string.IsNullOrWhiteSpace(order.Payment.PaymentIntentId))
        {
            var existing = await _pi.GetAsync(order.Payment.PaymentIntentId, cancellationToken: ct);
            return existing.ClientSecret;
        }

        var create = new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(order.GrandTotal),
            Currency = order.Currency.ToLowerInvariant(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new() { ["orderNumber"] = orderNumber, ["cartId"] = cartId }
        };

        var pi = await _pi.CreateAsync(create, new RequestOptions { IdempotencyKey = orderNumber }, ct);
        order.Payment.OnIntentCreated(pi.Id, pi.PaymentMethodTypes?.FirstOrDefault());
        await _db.SaveChangesAsync(ct);
        return pi.ClientSecret;
    }
}
