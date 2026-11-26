using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Policy = "Customer")]
public sealed class CartController(ICartService cart) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<CartDto>> Get(CancellationToken ct) => Ok(await cart.GetAsync(User.GetUserId(), ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> Add(AddCartItemRequest request, CancellationToken ct) =>
        Ok(await cart.AddItemAsync(User.GetUserId(), request, ct));

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> SetQuantity(Guid productId, UpdateCartItemRequest request, CancellationToken ct) =>
        Ok(await cart.SetQuantityAsync(User.GetUserId(), productId, request.Quantity, ct));

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> Remove(Guid productId, CancellationToken ct) =>
        Ok(await cart.RemoveItemAsync(User.GetUserId(), productId, ct));

    [HttpDelete] public async Task<ActionResult<CartDto>> Clear(CancellationToken ct) => Ok(await cart.ClearAsync(User.GetUserId(), ct));
}
