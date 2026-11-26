using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using QuickBasket.Api.Extensions;
using Microsoft.IdentityModel.Tokens;
using QuickBasket.Api.Middleware;
using QuickBasket.Application;
using QuickBasket.Infrastructure;
using QuickBasket.Infrastructure.Auth;
using QuickBasket.Infrastructure.Persistence;
using QuickBasket.Payments;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Optional local override file for secrets during development. It is git-ignored.
config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ---- Layers ----
builder.Services.AddApplication(config);
builder.Services.AddInfrastructure(config);
builder.Services.AddPaymentModule(config); // Payments:Provider = Mock | Razorpay

// ---- Authentication & authorization ----
var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be set (>= 32 chars) via environment variable, user-secrets or Key Vault.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // keep short claim names: sub, role, scope
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt.Issuer,
            ValidateAudience = true, ValidAudience = jwt.Audience,
            ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RoleClaimType = "role", NameClaimType = "name"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("Admin"))
    .AddPolicy("Customer", p => p.RequireRole("Customer"))
    .AddPolicy("Partner", p => p.RequireRole("DeliveryPartner"))
    // Starting or confirming a payment needs an interactive customer session carrying the "payments" scope.
    .AddPolicy("PaymentActions", p => p.RequireRole("Customer").RequireClaim("scope", JwtTokenService.PaymentsScope));

// ---- CORS (the React app) ----
var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

// ---- Rate limiting for credential and payment endpoints ----
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy("payments", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.FindFirst("sub")?.Value ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddControllers().AddJsonOptions(o =>
    // Accept and return enum names ("UpiQr", "Packed") instead of numbers, as the frontend and .http file send them.
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi(o => o.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                       // /openapi/v1.json
    app.UseSwaggerUI(o =>                   // /swagger  (interactive page over the same document)
    {
        o.SwaggerEndpoint("/openapi/v1.json", "QuickBasket API v1");
        o.RoutePrefix = "swagger";
        o.EnablePersistAuthorization();     // keeps the pasted token across page reloads
    });
}
else app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

await DbSeeder.InitializeAsync(app.Services, config);
await app.RunAsync();

public partial class Program; // exposed for integration tests
