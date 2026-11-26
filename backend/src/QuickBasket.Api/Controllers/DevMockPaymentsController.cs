using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Mock;
using QuickBasket.Payments.Models;

namespace QuickBasket.Api.Controllers;

/// <summary>
/// DEVELOPMENT ONLY. Lets you finish a payment without a gateway: it plays the part of the gateway and
/// sends a correctly SIGNED webhook through the same code path Razorpay would use.
/// Returns 404 unless the environment is Development AND Payments:Provider is Mock.
/// </summary>
[ApiController]
[Route("api/dev/mock-payments")]
[AllowAnonymous]
public sealed class DevMockPaymentsController(
    IWebHostEnvironment env, IPaymentProvider provider, IPaymentRepository payments, IPaymentWebhookService webhooks) : ControllerBase
{
    private MockPaymentProvider? Mock => env.IsDevelopment() ? provider as MockPaymentProvider : null;

    /// <summary>Pretend the customer paid (success=true) or the payment failed (success=false) for one of OUR payments.</summary>
    [HttpPost("{paymentId:guid}/simulate")]
    public async Task<IActionResult> Simulate(Guid paymentId, [FromQuery] bool success = true, CancellationToken ct = default)
    {
        if (Mock is not { } mock) return NotFound();
        var payment = await payments.GetByIdAsync(paymentId, ct);
        if (payment is null) return NotFound(new { error = "Payment not found." });

        var paise = Money.ToPaise(payment.Amount);
        GatewayPayment gp;
        string body;

        if (payment.GatewayQrId is not null)
        {
            if (!success) return BadRequest(new { error = "A UPI QR either gets paid or expires. Wait for the expiry or use success=true." });
            gp = mock.SimulateQrPayment(paise);
            body = mock.BuildQrCreditedWebhook(payment.GatewayQrId, gp);
        }
        else if (payment.GatewayOrderId is not null)
        {
            if (!mock.HasOrder(payment.GatewayOrderId))
                return Conflict(new { error = "The mock gateway forgot this order (the API restarted). Start a new payment." });
            gp = mock.SimulatePayment(payment.GatewayOrderId, success ? GatewayPaymentStatus.Captured : GatewayPaymentStatus.Failed);
            body = mock.BuildPaymentWebhook(success ? "payment.captured" : "payment.failed", gp);
        }
        else return BadRequest(new { error = "Cash on delivery has no gateway. Mark it delivered as a partner instead." });

        // Same entry point the real webhook uses: signature check, de-duplication, state change.
        var result = await webhooks.HandleAsync(body, mock.SignWebhook(body), Guid.NewGuid().ToString("N"), ct);

        return Ok(new
        {
            webhook = result,
            gatewayOrderId = gp.GatewayOrderId,
            gatewayPaymentId = gp.GatewayPaymentId,
            // Lets you also exercise POST /api/payments/verify (the "browser says paid" path).
            checkoutSignature = gp.GatewayOrderId is null ? null : mock.SignCheckout(gp.GatewayOrderId, gp.GatewayPaymentId)
        });
    }

    /// <summary>Pretend the gateway finished a pending refund.</summary>
    [HttpPost("refunds/{refundId:guid}/process")]
    public async Task<IActionResult> ProcessRefund(Guid refundId, CancellationToken ct = default)
    {
        if (Mock is not { } mock) return NotFound();
        var refund = await payments.GetRefundAsync(refundId, ct);
        if (refund?.GatewayRefundId is null) return NotFound(new { error = "Refund not found or has no gateway id." });
        var payment = await payments.GetByIdAsync(refund.PaymentId, ct);
        if (payment?.GatewayPaymentId is null) return NotFound(new { error = "Payment not found." });

        var body = mock.BuildRefundWebhook("refund.processed", refund.GatewayRefundId, payment.GatewayPaymentId, Money.ToPaise(refund.Amount), "processed");
        return Ok(await webhooks.HandleAsync(body, mock.SignWebhook(body), Guid.NewGuid().ToString("N"), ct));
    }
}
