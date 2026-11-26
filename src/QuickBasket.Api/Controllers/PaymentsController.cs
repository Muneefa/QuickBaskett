using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/payments")]
[EnableRateLimiting("payments")]
public sealed class PaymentsController(IPaymentService payments) : ControllerBase
{
    /// <summary>
    /// Starts (or safely replays) a payment for an order. Requires the explicit customer confirmation flag and a customer
    /// session with the "payments" scope. Send an Idempotency-Key header to make retries of the same request safe.
    /// Also used to retry after a failed or expired payment.
    /// </summary>
    [HttpPost("initiate"), Authorize(Policy = "PaymentActions")]
    public async Task<ActionResult<InitiatePaymentResponse>> Initiate(
        InitiatePaymentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken ct) =>
        Ok(await payments.InitiateAsync(User.GetUserId(), request, idempotencyKey, ct));

    /// <summary>Called by the browser after checkout. The result is re-checked against the gateway; the browser is never trusted.</summary>
    [HttpPost("verify"), Authorize(Policy = "PaymentActions")]
    public async Task<ActionResult<PaymentDto>> Verify(VerifyPaymentRequest request, CancellationToken ct) =>
        Ok(await payments.VerifyCheckoutAsync(User.GetUserId(), request, ct));

    /// <summary>Poll this for QR payments: status only changes once the gateway confirms through the API or a webhook.</summary>
    [HttpGet("{id:guid}"), Authorize(Policy = "Customer")]
    public async Task<ActionResult<PaymentDto>> Get(Guid id, CancellationToken ct) => Ok(await payments.GetMineAsync(User.GetUserId(), id, ct));

    [HttpGet("order/{orderId:guid}"), Authorize(Policy = "Customer")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> ForOrder(Guid orderId, CancellationToken ct) =>
        Ok(await payments.ListForOrderAsync(User.GetUserId(), orderId, ct));
}
