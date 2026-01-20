using Application.Interfaces;
using Domain.Stripe;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace Api.Controllers;

[ApiController]
[Route("/api/payments")]
public class PaymentController(PaymentsService payments,
    IOrderService orders,
    IInventoryService inventory,
    IOptions<StripeOptions> opts,
    ILogger<PaymentController> log) : ControllerBase
{
    private readonly PaymentsService _payments = payments;
    private readonly IOrderService _orderService = orders;
    private readonly IInventoryService _inventoryService = inventory;
    private readonly StripeOptions _options = opts.Value;

    public sealed record CreateIntentRequest(string OrderNumber, string CartId);
    public sealed record CreateIntentResponse(string ClientSecret);

    [HttpPost("create-intent")]
    public async Task<ActionResult<CreateIntentResponse>> CreateIntent([FromBody] CreateIntentRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.OrderNumber) || string.IsNullOrWhiteSpace(body.CartId))
            return BadRequest("Order number and Cart id is required");

        var secret = await _payments.CreateOrGetClientSecretAsync(body.OrderNumber, body.CartId, ct);
        return Ok(new CreateIntentResponse(secret));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        log.LogWarning("=== STRIPE WEBHOOK HIT === {UtcNow}", DateTime.UtcNow);

        var json = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        log.LogInformation("Webhook payload length: {Len}", json.Length);

        Event e;
        try
        {
            var sig = Request.Headers["Stripe-Signature"].ToString();
            log.LogInformation("Stripe-Signature header present: {Present}", !string.IsNullOrWhiteSpace(sig));

            e = EventUtility.ConstructEvent(json, sig, _options.WebhookSecret);
            log.LogInformation("Stripe event parsed: Type={Type} Id={Id}", e.Type, e.Id);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Stripe signature validation failed");
            return BadRequest("Invalid signature");
        }

        switch (e.Type)
        {
            case "payment_intent.succeeded":
            case "payment_intent.processing":
                {
                    var pi = (PaymentIntent)e.Data.Object;

                    var orderNumber = pi.Metadata.TryGetValue("orderNumber", out var on) ? on : null;
                    var cartId = pi.Metadata.TryGetValue("cartId", out var cid) ? cid : null;

                    log.LogInformation("PI={PiId} orderNumber={OrderNumber} cartId={CartId} amount={Amount} status={Status}",
                        pi.Id, orderNumber, cartId, pi.AmountReceived > 0 ? pi.AmountReceived : pi.Amount, pi.Status);

                    if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(cartId))
                    {
                        log.LogWarning("Missing metadata. Skipping order update.");
                        return Ok();
                    }

                    var methodType = pi.PaymentMethodTypes?.FirstOrDefault();
                    var latestChargeId = pi.LatestChargeId;
                    var amount = (decimal)(pi.AmountReceived > 0 ? pi.AmountReceived : pi.Amount) / 100m;

                    var ok = await _orderService.MarkPaymentAuthorizedAsync(orderNumber, pi.Id, latestChargeId, methodType, amount, cartId, ct);
                    log.LogWarning("MarkPaymentAuthorizedAsync result: {Ok}", ok);

                    return Ok();
                }

            case "payment_intent.payment_failed":
            case "payment_intent.canceled":
                {
                    var pi = (PaymentIntent)e.Data.Object;
                    var orderNumber = pi.Metadata.TryGetValue("orderNumber", out var on) ? on : null;
                    var cartId = pi.Metadata.TryGetValue("cartId", out var cid) ? cid : null;

                    log.LogWarning("Payment failed/canceled. PI={PiId} orderNumber={OrderNumber} cartId={CartId}",
                        pi.Id, orderNumber, cartId);

                    if (!string.IsNullOrWhiteSpace(orderNumber))
                        await _orderService.MarkPaymentFailedAsync(orderNumber, ct);

                    if (!string.IsNullOrWhiteSpace(cartId))
                        await _inventoryService.ReleaseExpiredAsync(ct);

                    return Ok();
                }

            default:
                log.LogInformation("Unhandled event: {Type}", e.Type);
                return Ok();
        }
    }
}


