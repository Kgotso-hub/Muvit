namespace Muvit.Domain.ValueObjects;

public sealed record VehicleInfo
{
    public string VehicleType { get; }
    public string PlateNumber { get; }

    public VehicleInfo(string vehicleType, string plateNumber)
    {
        if (string.IsNullOrWhiteSpace(vehicleType))
            throw new ArgumentException("Vehicle type is required.", nameof(vehicleType));
        if (string.IsNullOrWhiteSpace(plateNumber))
            throw new ArgumentException("Plate number is required.", nameof(plateNumber));

        VehicleType = vehicleType;
        PlateNumber = plateNumber;
    }
}