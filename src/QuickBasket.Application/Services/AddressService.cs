using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;

namespace QuickBasket.Application.Services;

public sealed class AddressService(IAddressRepository addresses, IUnitOfWork uow) : IAddressService
{
    public async Task<IReadOnlyList<AddressDto>> ListAsync(Guid userId, CancellationToken ct = default) =>
        (await addresses.ListByUserAsync(userId, ct)).Select(a => a.ToDto()).ToList();

    public async Task<AddressDto> CreateAsync(Guid userId, SaveAddressRequest r, CancellationToken ct = default)
    {
        Validate(r);
        var address = new Address { UserId = userId };
        Apply(address, r);
        await addresses.AddAsync(address, ct);
        await ClearOtherDefaultsAsync(userId, address, ct);
        await uow.SaveChangesAsync(ct);
        return address.ToDto();
    }

    public async Task<AddressDto> UpdateAsync(Guid userId, Guid id, SaveAddressRequest r, CancellationToken ct = default)
    {
        Validate(r);
        var address = await addresses.GetAsync(id, userId, ct) ?? throw new NotFoundException("Address not found.");
        Apply(address, r);
        address.UpdatedAtUtc = DateTime.UtcNow;
        await ClearOtherDefaultsAsync(userId, address, ct);
        await uow.SaveChangesAsync(ct);
        return address.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var address = await addresses.GetAsync(id, userId, ct) ?? throw new NotFoundException("Address not found.");
        addresses.Remove(address);
        await uow.SaveChangesAsync(ct);
    }

    private async Task ClearOtherDefaultsAsync(Guid userId, Address current, CancellationToken ct)
    {
        if (!current.IsDefault) return;
        foreach (var other in await addresses.ListByUserAsync(userId, ct))
            if (other.Id != current.Id) other.IsDefault = false;
    }

    private static void Apply(Address a, SaveAddressRequest r)
    {
        a.Label = r.Label; a.RecipientName = r.RecipientName.Trim(); a.Phone = r.Phone.Trim();
        a.Line1 = r.Line1.Trim(); a.Line2 = r.Line2?.Trim(); a.City = r.City.Trim();
        a.State = r.State.Trim(); a.PostalCode = r.PostalCode.Trim(); a.IsDefault = r.IsDefault;
    }

    private static void Validate(SaveAddressRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.RecipientName) || string.IsNullOrWhiteSpace(r.Line1) ||
            string.IsNullOrWhiteSpace(r.City) || string.IsNullOrWhiteSpace(r.PostalCode) || string.IsNullOrWhiteSpace(r.Phone) ||
            string.IsNullOrWhiteSpace(r.State))
            throw new ValidationException("Recipient, phone, address line 1, city, state and postal code are required.");
    }
}
