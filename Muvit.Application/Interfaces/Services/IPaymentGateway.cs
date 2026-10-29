using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Interfaces.Services;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(Money amount, string paymentMethodToken);
    Task<PaymentGatewayResult> RefundAsync(string gatewayReference, Money amount);
}

public sealed record PaymentGatewayResult(bool Succeeded, string? GatewayReference, string? ErrorMessage);