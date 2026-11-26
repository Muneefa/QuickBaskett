using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.Services;

public sealed class AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens, IUnitOfWork uow) : IAuthService
{
    public async Task<AuthResponse> RegisterCustomerAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var user = await CreateUserAsync(r.Email, r.Password, r.FullName, r.PhoneNumber, UserRole.Customer, ct);
        return Issue(user);
    }

    public async Task<UserDto> CreatePartnerAsync(CreatePartnerRequest r, CancellationToken ct = default)
    {
        var user = await CreateUserAsync(r.Email, r.Password, r.FullName, r.PhoneNumber, UserRole.DeliveryPartner, ct);
        return user.ToDto();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(Normalize(r.Email), ct);
        // One generic message for unknown email and wrong password so accounts cannot be enumerated.
        if (user is null || !user.IsActive || !hasher.Verify(r.Password ?? string.Empty, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid email or password.");
        return Issue(user);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken ct = default) =>
        (await users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.")).ToDto();

    public async Task<IReadOnlyList<UserDto>> ListPartnersAsync(CancellationToken ct = default) =>
        (await users.ListByRoleAsync(UserRole.DeliveryPartner, ct)).Select(u => u.ToDto()).ToList();

    private async Task<User> CreateUserAsync(string email, string password, string fullName, string? phone, UserRole role, CancellationToken ct)
    {
        email = Normalize(email);
        if (!email.Contains('@') || email.Length > 256) throw new ValidationException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8) throw new ValidationException("Password must be at least 8 characters.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ValidationException("Full name is required.");
        if (await users.GetByEmailAsync(email, ct) is not null) throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            FullName = fullName.Trim(),
            PhoneNumber = phone?.Trim(),
            Role = role,
            IsAvailable = role == UserRole.DeliveryPartner
        };
        await users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return user;
    }

    private AuthResponse Issue(User user)
    {
        var token = tokens.CreateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, user.ToDto());
    }

    private static string Normalize(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}
