using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Services;

public sealed class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentService(IPaymentRepository paymentRepository, IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Payment> ProcessPaymentAsync(Guid bookingId, Guid customerId, Money amount, string paymentMethodToken, string? gatewayName = null)
    {
        var payment = Payment.Initiate(bookingId, customerId, amount, gatewayName);
        await _paymentRepository.AddAsync(payment);

        var result = await _paymentGateway.ChargeAsync(amount, paymentMethodToken);

        if (result.Succeeded)
            payment.MarkCompleted(result.GatewayReference!);
        else
            payment.MarkFailed();

        await _paymentRepository.UpdateAsync(payment);

        return payment;
    }
}