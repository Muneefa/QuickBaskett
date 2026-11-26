using QuickBasket.Domain.Entities;

namespace QuickBasket.Application.Abstractions.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public sealed record IssuedToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    IssuedToken CreateToken(User user);
}
