using Domain.Stripe;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Services;

public class PaymentsService(PallshoppenDbContext dbContext, IOptions<StripeOptions> opts)
{
    private readonly PallshoppenDbContext _dbContext = dbContext;
    private readonly PaymentIntentService _pi = new PaymentIntentService();
    private readonly StripeOptions _options = opts.Value;

    private static long ToMinorUnits(decimal amount) =>
        (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    public async Task<string> CreateOrGetClientSecretAsync(string orderNumber, string cartId, CancellationToken ct)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct)
                   ?? throw new InvalidOperationException("Order not found");

        if (!string.IsNullOrWhiteSpace(order.Payment.PaymentIntentId))
        {
            var existing = await _pi.GetAsync(order.Payment.PaymentIntentId, cancellationToken: ct);
            return existing.ClientSecret;
        }

        var create = new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(order.GrandTotal),
            Currency = (order.Currency ?? "SEK").ToLowerInvariant(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new() { ["orderNumber"] = orderNumber, ["cartId"] = cartId },
            Shipping = new ChargeShippingOptions
            {
                Name = $"{order.CustomerFirstName} {order.CustomerLastName}",
                Address = new AddressOptions
                {
                    Line1 = order.ShippingAddress.Street,
                    PostalCode = order.ShippingAddress.PostalCode,
                    City = order.ShippingAddress.City,
                    Country = order.ShippingAddress.Country,

                },
                Phone = order.CustomerPhoneNumber
            }

        };

        var pi = await _pi.CreateAsync(create, new RequestOptions { IdempotencyKey = orderNumber }, ct);
        order.Payment.OnIntentCreated(pi.Id,pi.PaymentMethodTypes?.FirstOrDefault()  
        );

        await _dbContext.SaveChangesAsync(ct);
        return pi.ClientSecret;
    }
}
