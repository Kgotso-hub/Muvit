using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class Driver : User
{
    public DriverProfile Profile { get; private init; }

    public Driver(string fullName, string email, string phoneNumber, string passwordHash, VehicleInfo vehicle)
        : base(fullName, email, phoneNumber, passwordHash)
    {
        Profile = new DriverProfile(vehicle);
    }

    public override UserRole Role => UserRole.Driver;
}