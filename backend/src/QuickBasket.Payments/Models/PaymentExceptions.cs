namespace QuickBasket.Payments.Models;

public class PaymentGatewayException(string message, int? statusCode = null, Exception? inner = null)
    : Exception(message, inner)
{
    public int? StatusCode { get; } = statusCode;
}

public class PaymentConfigurationException(string message) : Exception(message);

public class InvalidWebhookException(string message, Exception? inner = null) : Exception(message, inner);
