using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "Admin")]
public sealed class AdminController(
    IOrderService orders, IPaymentService payments, IRefundService refunds, IAuthService auth) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<OrderDto>>> Orders(
        [FromQuery] OrderStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await orders.ListAllAsync(status, page, pageSize, ct));

    [HttpPut("orders/{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct) =>
        Ok(await orders.UpdateStatusAsync(id, request, ct));

    [HttpPut("orders/{id:guid}/assign")]
    public async Task<ActionResult<OrderDto>> Assign(Guid id, AssignPartnerRequest request, CancellationToken ct) =>
        Ok(await orders.AssignPartnerAsync(id, request.PartnerId, ct));

    [HttpGet("payments")]
    public async Task<ActionResult<PagedResult<PaymentDto>>> Payments(
        [FromQuery] PaymentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await payments.ListAllAsync(status, page, pageSize, ct));

    [HttpGet("payments/{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Payment(Guid id, CancellationToken ct) => Ok(await payments.GetAsAdminAsync(id, ct));

    /// <summary>Refunds need an Idempotency-Key header so a double click cannot refund twice.</summary>
    [HttpPost("payments/{id:guid}/refunds")]
    public async Task<ActionResult<RefundDto>> Refund(
        Guid id, RefundRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken ct) =>
        Ok(await refunds.RequestAsync(User.GetUserId(), id, request, idempotencyKey, ct));

    [HttpGet("partners")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Partners(CancellationToken ct) => Ok(await auth.ListPartnersAsync(ct));

    [HttpPost("partners")]
    public async Task<ActionResult<UserDto>> CreatePartner(CreatePartnerRequest request, CancellationToken ct) =>
        Ok(await auth.CreatePartnerAsync(request, ct));
}
