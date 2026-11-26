using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Mock;
using QuickBasket.Payments.Razorpay;

namespace QuickBasket.Payments;

public static class PaymentModuleExtensions
{
    /// <summary>
    /// Registers the configured provider. Payments:Provider = "Mock" (default) or "Razorpay".
    /// </summary>
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Payments:Provider"] ?? "Mock";

        if (provider.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<RazorpayOptions>(config.GetSection(RazorpayOptions.SectionName));
            services.AddHttpClient<RazorpayPaymentProvider>((sp, client) =>
            {
                var o = sp.GetRequiredService<IOptions<RazorpayOptions>>().Value;
                RazorpayPaymentProvider.Validate(o); // fail fast on missing keys or blocked live keys
                client.BaseAddress = new Uri(o.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{o.KeyId}:{o.KeySecret}")));
            });
            services.AddTransient<IPaymentProvider>(sp => sp.GetRequiredService<RazorpayPaymentProvider>());
        }
        else
        {
            var mockOptions = config.GetSection(MockPaymentOptions.SectionName).Get<MockPaymentOptions>() ?? new MockPaymentOptions();
            services.AddSingleton(mockOptions);
            services.AddSingleton<MockPaymentProvider>();
            services.AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<MockPaymentProvider>());
        }

        return services;
    }
}
