using Muvit.Domain.Enums;

namespace Muvit.Domain.Entities;

public sealed class Dispute
{
    public Guid Id { get; }
    public Guid BookingId { get; }
    public Guid RaisedByUserId { get; }
    public Guid AgainstUserId { get; }
    public string Description { get; }
    public DisputeStatus Status { get; private set; }
    public DisputeResolutionOutcome? Outcome { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public Guid? ResolvedByAdminId { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? ResolvedAt { get; private set; }

    private Dispute(Guid bookingId, Guid raisedByUserId, Guid againstUserId, string description)
    {
        if (raisedByUserId == againstUserId)
            throw new ArgumentException("A user cannot raise a dispute against themselves.", nameof(againstUserId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A dispute needs a description.", nameof(description));

        Id = Guid.NewGuid();
        BookingId = bookingId;
        RaisedByUserId = raisedByUserId;
        AgainstUserId = againstUserId;
        Description = description;
        Status = DisputeStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public static Dispute Raise(Guid bookingId, Guid raisedByUserId, Guid againstUserId, string description)
        => new(bookingId, raisedByUserId, againstUserId, description);

    public void StartInvestigating()
    {
        if (Status != DisputeStatus.Open)
            throw new InvalidOperationException($"Cannot start investigating a {Status} dispute — it must be Open first.");

        Status = DisputeStatus.Investigating;
    }

    public void Resolve(Guid resolvedByAdminId, DisputeResolutionOutcome outcome, string resolutionNotes)
    {
        if (Status == DisputeStatus.Resolved || Status == DisputeStatus.Dismissed)
            throw new InvalidOperationException($"Cannot resolve a dispute that is already {Status}.");

        if (string.IsNullOrWhiteSpace(resolutionNotes))
            throw new ArgumentException("Resolution notes are required.", nameof(resolutionNotes));

        Outcome = outcome;
        ResolutionNotes = resolutionNotes;
        ResolvedByAdminId = resolvedByAdminId;
        ResolvedAt = DateTime.UtcNow;

        // "Dismiss" is one of the four outcomes, but it closes the dispute via the Dismissed
        // status rather than Resolved — the other three outcomes close it as Resolved.
        Status = outcome == DisputeResolutionOutcome.Dismiss ? DisputeStatus.Dismissed : DisputeStatus.Resolved;
    }
}