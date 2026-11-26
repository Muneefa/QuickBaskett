using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/partner")]
[Authorize(Policy = "Partner")]
public sealed class PartnerController(IOrderService orders) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Assigned(CancellationToken ct) => Ok(await orders.ListAssignedAsync(User.GetUserId(), ct));

    [HttpPost("orders/{id:guid}/pickup")]
    public async Task<ActionResult<OrderDto>> PickUp(Guid id, CancellationToken ct) => Ok(await orders.PickUpAsync(User.GetUserId(), id, ct));

    [HttpPost("orders/{id:guid}/deliver")]
    public async Task<ActionResult<OrderDto>> Deliver(Guid id, DeliverOrderRequest request, CancellationToken ct) =>
        Ok(await orders.DeliverAsync(User.GetUserId(), id, request, ct));
}
