namespace QuickBasket.Application.DTOs;

public sealed record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber);
public sealed record LoginRequest(string Email, string Password);
public sealed record CreatePartnerRequest(string Email, string Password, string FullName, string? PhoneNumber);
public sealed record UserDto(Guid Id, string Email, string FullName, string? PhoneNumber, string Role, bool IsAvailable);
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
