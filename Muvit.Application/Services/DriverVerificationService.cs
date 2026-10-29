using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;

namespace Muvit.Application.Services;

public sealed class DriverVerificationService
{
    private readonly IUserRepository _userRepository;

    public DriverVerificationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ApproveAsync(Guid driverId)
    {
        var driver = await GetDriverAsync(driverId);

        driver.Profile.Approve();

        await _userRepository.UpdateAsync(driver);
    }

    public async Task RejectAsync(Guid driverId)
    {
        var driver = await GetDriverAsync(driverId);

        driver.Profile.Reject();

        await _userRepository.UpdateAsync(driver);
    }

    private async Task<Driver> GetDriverAsync(Guid driverId)
    {
        var user = await _userRepository.GetByIdAsync(driverId)
            ?? throw new InvalidOperationException($"No user found with id {driverId}.");

        if (user is not Driver driver)
            throw new InvalidOperationException($"User {driverId} is not a driver.");

        return driver;
    }
}