using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Api.Extensions;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize(Policy = "Customer")]
public sealed class AddressesController(IAddressService addresses) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<AddressDto>>> List(CancellationToken ct) => Ok(await addresses.ListAsync(User.GetUserId(), ct));

    [HttpPost] public async Task<ActionResult<AddressDto>> Create(SaveAddressRequest request, CancellationToken ct) =>
        Ok(await addresses.CreateAsync(User.GetUserId(), request, ct));

    [HttpPut("{id:guid}")] public async Task<ActionResult<AddressDto>> Update(Guid id, SaveAddressRequest request, CancellationToken ct) =>
        Ok(await addresses.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await addresses.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
