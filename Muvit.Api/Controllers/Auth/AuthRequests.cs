using Muvit.Domain.Enums;

namespace Muvit.Api.Controllers.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    UserRole Role,
    string? VehicleType = null,
    string? PlateNumber = null);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, Guid UserId, string FullName, string Email, UserRole Role);