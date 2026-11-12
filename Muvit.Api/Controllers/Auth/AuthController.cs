using Microsoft.AspNetCore.Mvc;
using Muvit.Application.Services;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            VehicleInfo? vehicle = null;
            if (request.Role == UserRole.Driver
                && !string.IsNullOrWhiteSpace(request.VehicleType)
                && !string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                vehicle = new VehicleInfo(request.VehicleType, request.PlateNumber);
            }

            var (user, token) = await _authService.RegisterAsync(
                request.FullName, request.Email, request.PhoneNumber, request.Password, request.Role, vehicle);

            return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, user.Role));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            var (user, token) = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, user.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}