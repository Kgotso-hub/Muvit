using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class DriverProfile
{
    public VehicleInfo Vehicle { get; private set; }
    public VerificationStatus Verification { get; private set; }
    public AvailabilityStatus Availability { get; private set; }
    public decimal? AverageRating { get; private set; }
    public int RatingCount { get; private set; }

    public DriverProfile(VehicleInfo vehicle)
    {
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        Verification = VerificationStatus.Pending;
        Availability = AvailabilityStatus.Offline; // can't go online before being verified
        AverageRating = null;
        RatingCount = 0;
    }

    public void Approve() => Verification = VerificationStatus.Approved;

    public void Reject()
    {
        Verification = VerificationStatus.Rejected;
        Availability = AvailabilityStatus.Offline;
    }

    public void GoOnline()
    {
        if (Verification != VerificationStatus.Approved)
            throw new InvalidOperationException("Only an approved driver can go online.");

        Availability = AvailabilityStatus.Online;
    }

    public void GoOffline() => Availability = AvailabilityStatus.Offline;

    public void UpdateVehicle(VehicleInfo vehicle) =>
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));

    public void RecordRating(int score)
    {
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Rating must be between 1 and 5.");

        var totalScore = (AverageRating ?? 0) * RatingCount + score;
        RatingCount++;
        AverageRating = totalScore / RatingCount;
    }
}