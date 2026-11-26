using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickBasket.Application.Abstractions.Services;

namespace QuickBasket.Infrastructure.Workers;

/// <summary>Marks unpaid attempts whose window passed as Expired so the checkout can offer a retry.</summary>
public sealed class PaymentExpiryWorker(IServiceScopeFactory scopes, ILogger<PaymentExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var expired = await scope.ServiceProvider.GetRequiredService<IPaymentService>().ExpireOverdueAsync(DateTime.UtcNow, stoppingToken);
                if (expired > 0) logger.LogInformation("Expired {Count} overdue payment attempts", expired);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Payment expiry sweep failed"); }
        }
    }
}
