using Microsoft.AspNetCore.Mvc;
using QuickBasket.Application.Common;
using QuickBasket.Payments.Models;

namespace QuickBasket.Api.Middleware;

/// <summary>Maps application exceptions to RFC 7807 problem responses. Internal details are logged, never returned.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, expose) = ex switch
        {
            NotFoundException => (404, "Not found", true),
            ValidationException => (400, "Validation failed", true),
            ConflictException => (409, "Conflict", true),
            DuplicateRecordException => (409, "Duplicate request", true),
            ConcurrencyConflictException => (409, "Concurrent update, please retry", false),
            ForbiddenException => (403, "Forbidden", true),
            UnauthorizedAppException => (401, "Unauthorized", true),
            PaymentMismatchException => (409, "Payment mismatch", true),
            PaymentGatewayException => (502, "Payment gateway error", false),
            PaymentConfigurationException => (503, "Payments are not available", false),
            _ => (500, "Unexpected error", false)
        };

        if (status >= 500) logger.LogError(ex, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);
        else logger.LogInformation("{Status} {Title}: {Message}", status, title, ex.Message);

        if (context.Response.HasStarted) return; // too late to change the response

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = expose ? ex.Message : null,
            Instance = context.Request.Path
        });
    }
}
