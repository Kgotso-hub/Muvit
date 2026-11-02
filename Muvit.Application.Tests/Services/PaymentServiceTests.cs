using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Application.Services;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IPaymentGateway> _paymentGateway = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(_paymentRepository.Object, _paymentGateway.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_GatewaySucceeds_MarksPaymentCompleted()
    {
        var bookingId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = new Money(500m, "ZAR");

        _paymentGateway
            .Setup(g => g.ChargeAsync(amount, "tok_test"))
            .ReturnsAsync(new PaymentGatewayResult(true, "ch_123", null));

        var payment = await _sut.ProcessPaymentAsync(bookingId, customerId, amount, "tok_test");

        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("ch_123", payment.GatewayReference);
        _paymentRepository.Verify(r => r.AddAsync(payment), Times.Once);
        _paymentRepository.Verify(r => r.UpdateAsync(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_GatewayFails_MarksPaymentFailed()
    {
        var bookingId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = new Money(500m, "ZAR");

        _paymentGateway
            .Setup(g => g.ChargeAsync(amount, "tok_test"))
            .ReturnsAsync(new PaymentGatewayResult(false, null, "Card declined."));

        var payment = await _sut.ProcessPaymentAsync(bookingId, customerId, amount, "tok_test");

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        _paymentRepository.Verify(r => r.AddAsync(payment), Times.Once);
        _paymentRepository.Verify(r => r.UpdateAsync(payment), Times.Once);
    }
}