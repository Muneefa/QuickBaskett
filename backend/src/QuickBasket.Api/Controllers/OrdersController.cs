using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = "Customer")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    /// <summary>Creates an order (status PendingPayment) from the server-side cart. The total is calculated here, not by the client.</summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request, CancellationToken ct) =>
        Ok(await orders.CheckoutAsync(User.GetUserId(), request, ct));

    [HttpGet] public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(CancellationToken ct) => Ok(await orders.ListMineAsync(User.GetUserId(), ct));

    [HttpGet("{id:guid}")] public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct) => Ok(await orders.GetMineAsync(User.GetUserId(), id, ct));

    [HttpPost("{id:guid}/cancel")] public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken ct) =>
        Ok(await orders.CancelMineAsync(User.GetUserId(), id, ct));
}
