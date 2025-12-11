using Application.Interfaces;
using Domain.Stripe;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace Api.Controllers;

[ApiController]
[Route("/api/payments")]
public class PaymentController(PaymentsService payments, IOrderService orders, IInventoryService inventory, IOptions<StripeOptions> opts) : ControllerBase
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
        var json = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        Event e;
        try
        {
            var sig = Request.Headers["Stripe-Signature"];
            e = EventUtility.ConstructEvent(json, sig, _options.WebhookSecret);
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid Signature {ex.Message}");
        }

        switch (e.Type)
        {
            case "payment_intent.succeeded":
            case "payment_intent.processing":
                {
                    var pi = (PaymentIntent)e.Data.Object;

                    var orderNumber = pi.Metadata.TryGetValue("orderNumber", out var on) ? on : null;
                    var cartId = pi.Metadata.TryGetValue("cartId", out var cid) ? cid : null;
                    if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(cartId))
                        return Ok();

                    var methodType = pi.PaymentMethodTypes?.FirstOrDefault();
                    var latestChargeId = pi.LatestChargeId; 
                    var amount = (decimal)(pi.AmountReceived > 0 ? pi.AmountReceived : pi.Amount) / 100m;

                    await _orderService.MarkPaymentAuthorizedAsync(orderNumber, pi.Id, latestChargeId, methodType, amount, cartId, ct);
                    return Ok();
                }

            case "payment_intent.payment_failed":
            case "payment_intent.canceled":
                {
                    var pi = (PaymentIntent)e.Data.Object;

                    var orderNumber = pi.Metadata.TryGetValue("orderNumber", out var on) ? on : null;
                    var cartId = pi.Metadata.TryGetValue("cartId", out var cid) ? cid : null;

                    if (!string.IsNullOrWhiteSpace(orderNumber))
                        await _orderService.MarkPaymentFailedAsync(orderNumber, ct);

                    if (!string.IsNullOrWhiteSpace(cartId))
                        await _inventoryService.ReleaseExpiredAsync(ct); 

                    return Ok();
                }

            default:
                return Ok();
        }
    }

}
