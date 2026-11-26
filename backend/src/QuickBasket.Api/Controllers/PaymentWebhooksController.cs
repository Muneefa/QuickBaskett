using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous] // authenticity comes from the signature, not a bearer token
public sealed class PaymentWebhooksController(IPaymentWebhookService webhooks) : ControllerBase
{
    /// <summary>
    /// The signature is computed over the exact bytes Razorpay sent, so the body is read raw here and never
    /// bound to a model (re-serialising JSON would change the bytes and break verification).
    /// </summary>
    [HttpPost("razorpay")]
    public async Task<ActionResult<WebhookResult>> Razorpay(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync(ct);

        var result = await webhooks.HandleAsync(
            rawBody,
            Request.Headers["X-Razorpay-Signature"].FirstOrDefault(),
            Request.Headers["X-Razorpay-Event-Id"].FirstOrDefault(),
            ct);

        return Ok(result); // 2xx stops gateway retries; duplicates and ignored events are also acknowledged
    }
}
