using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Application.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class PayoutServiceTests
{
    private readonly Mock<IPayoutRepository> _payoutRepository = new();
    private readonly Mock<ICommissionStrategy> _commissionStrategy = new();
    private readonly PayoutService _sut;

    public PayoutServiceTests()
    {
        _sut = new PayoutService(_payoutRepository.Object, _commissionStrategy.Object);
    }

    [Fact]
    public async Task CalculatePayoutAsync_UsesStrategyRate_ComputesCommissionAndNet()
    {
        var paymentId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var grossAmount = new Money(1000m, "ZAR");

        _commissionStrategy.Setup(s => s.GetCommissionRate(grossAmount)).Returns(15m);

        var payout = await _sut.CalculatePayoutAsync(paymentId, bookingId, driverId, grossAmount);

        Assert.Equal(150m, payout.CommissionAmount.Amount);
        Assert.Equal(850m, payout.NetPayoutAmount.Amount);
        Assert.Equal(PayoutStatus.Pending, payout.Status);
        _payoutRepository.Verify(r => r.AddAsync(payout), Times.Once);
    }

    [Fact]
    public async Task ProcessPayoutAsync_ExistingPayout_MarksProcessedAndSaves()
    {
        var adminId = Guid.NewGuid();
        var payout = Payout.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(1000m, "ZAR"), 15m);
        _payoutRepository.Setup(r => r.GetByIdAsync(payout.Id)).ReturnsAsync(payout);

        var result = await _sut.ProcessPayoutAsync(payout.Id, adminId);

        Assert.Equal(PayoutStatus.Processed, result.Status);
        Assert.Equal(adminId, result.ProcessedByAdminId);
        _payoutRepository.Verify(r => r.UpdateAsync(payout), Times.Once);
    }
}