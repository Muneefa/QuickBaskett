using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickBasket.Application.Common;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Payments;
using QuickBasket.Application.Services;

namespace QuickBasket.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<OrderPricingOptions>(config.GetSection(OrderPricingOptions.SectionName));
        services.Configure<PaymentFlowOptions>(config.GetSection(PaymentFlowOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<PaymentStateUpdater>();
        return services;
    }
}
