using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Services;

public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<(User User, string Token)> RegisterAsync(
        string fullName, string email, string phoneNumber, string password, UserRole role, VehicleInfo? vehicle = null)
    {
        if (role == UserRole.Admin)
            throw new ArgumentException("Admin accounts cannot be created through registration.", nameof(role));

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException($"An account with email '{email}' already exists.");

        var passwordHash = _passwordHasher.HashPassword(password);

        User user = role switch
        {
            UserRole.Customer => new Customer(fullName, email, phoneNumber, passwordHash),
            UserRole.Driver => new Driver(fullName, email, phoneNumber, passwordHash,
                vehicle ?? throw new ArgumentException("Vehicle information is required to register as a driver.", nameof(vehicle))),
            _ => throw new ArgumentException($"Unsupported role: {role}.", nameof(role))
        };

        await _userRepository.AddAsync(user);

        var token = _tokenGenerator.GenerateToken(user);
        return (user, token);
    }

    public async Task<(User User, string Token)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _tokenGenerator.GenerateToken(user);
        return (user, token);
    }
}