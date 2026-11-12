using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Application.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepository.Object, _passwordHasher.Object, _tokenGenerator.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewCustomer_HashesPasswordSavesAndReturnsToken()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com")).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.HashPassword("plaintext")).Returns("hashed");
        _tokenGenerator.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("fake-jwt");

        var (user, token) = await _sut.RegisterAsync("Jane Doe", "jane@example.com", "0821234567", "plaintext", UserRole.Customer);

        Assert.Equal("fake-jwt", token);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.Equal("hashed", user.PasswordHash);
        _userRepository.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsAndNeverSaves()
    {
        var existing = new Customer("Existing User", "jane@example.com", "0821234567", "somehash");
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com")).ReturnsAsync(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RegisterAsync("Jane Doe", "jane@example.com", "0821234567", "plaintext", UserRole.Customer));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_AdminRole_ThrowsAndNeverChecksEmailOrSaves()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.RegisterAsync("Would-Be Admin", "admin@example.com", "0821234567", "plaintext", UserRole.Admin));

        _userRepository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_DriverWithoutVehicle_ThrowsAndNeverSaves()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("driver@example.com")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.RegisterAsync("Dan Driver", "driver@example.com", "0821234567", "plaintext", UserRole.Driver));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_NewDriver_ConstructsWithVehicleAndSaves()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("driver@example.com")).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.HashPassword("plaintext")).Returns("hashed");
        _tokenGenerator.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("fake-jwt");

        var vehicle = new VehicleInfo("Sedan", "CA123456");
        var (user, _) = await _sut.RegisterAsync("Dan Driver", "driver@example.com", "0821234567", "plaintext", UserRole.Driver, vehicle);

        var driver = Assert.IsType<Driver>(user);
        Assert.Equal("Sedan", driver.Profile.Vehicle.VehicleType);
        _userRepository.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new Customer("Jane Doe", "jane@example.com", "0821234567", "hashed");
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("plaintext", "hashed")).Returns(true);
        _tokenGenerator.Setup(t => t.GenerateToken(user)).Returns("fake-jwt");

        var (returnedUser, token) = await _sut.LoginAsync("jane@example.com", "plaintext");

        Assert.Equal(user, returnedUser);
        Assert.Equal("fake-jwt", token);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedAndNeverVerifiesOrGeneratesToken()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("ghost@example.com")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync("ghost@example.com", "plaintext"));

        _passwordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGenerator.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAndNeverGeneratesToken()
    {
        var user = new Customer("Jane Doe", "jane@example.com", "0821234567", "hashed");
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("wrong", "hashed")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync("jane@example.com", "wrong"));

        _tokenGenerator.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}