using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "QuickBasket";
    public string Audience { get; set; } = "QuickBasket.Clients";
    /// <summary>At least 32 characters. Supply through environment variables / Key Vault; never commit a real key.</summary>
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    public bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch (BCrypt.Net.SaltParseException) { return false; }
    }
}

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public const string PaymentsScope = "payments";

    private readonly JwtOptions _o = options.Value;

    public IssuedToken CreateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_o.SigningKey) || _o.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be configured with at least 32 characters.");

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("name", user.FullName),
            new("role", user.Role.ToString())
        };

        // Only interactive customer sessions can start or confirm payments. Tokens for any future automated
        // assistant must be issued WITHOUT this scope, so the AI can never initiate or approve a payment.
        if (user.Role == UserRole.Customer) claims.Add(new Claim("scope", PaymentsScope));

        var expires = DateTime.UtcNow.AddMinutes(_o.ExpiryMinutes);
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, notBefore: DateTime.UtcNow, expires: expires, signingCredentials: creds);
        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
