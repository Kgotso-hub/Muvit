using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class DriverVerificationServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly DriverVerificationService _sut;

    public DriverVerificationServiceTests()
    {
        _sut = new DriverVerificationService(_userRepository.Object);
    }

    [Fact]
    public async Task ApproveAsync_ExistingDriver_ApprovesAndSaves()
    {
        var driver = CreateDriver();
        _userRepository.Setup(r => r.GetByIdAsync(driver.Id)).ReturnsAsync(driver);

        await _sut.ApproveAsync(driver.Id);

        Assert.Equal(VerificationStatus.Approved, driver.Profile.Verification);
        _userRepository.Verify(r => r.UpdateAsync(driver), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_UserIsNotADriver_ThrowsAndNeverSaves()
    {
        var customer = new Customer("Thabo Mokoena", "thabo@example.com", "0821234567", "hashed-password");
        _userRepository.Setup(r => r.GetByIdAsync(customer.Id)).ReturnsAsync(customer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ApproveAsync(customer.Id));

        _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    private static Driver CreateDriver() =>
        new("Sipho Nkosi", "sipho@example.com", "0837654321", "hashed-password",
            new VehicleInfo("Bakkie", "CA 123-456"));
}