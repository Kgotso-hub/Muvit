using Muvit.Domain.Enums;

namespace Muvit.Domain.Entities;

public sealed class BookingStatusEvent
{
    public Guid Id { get; }
    public BookingStatus Status { get; }
    public Guid ChangedByUserId { get; }
    public string? Note { get; }
    public DateTime OccurredAt { get; }

    internal BookingStatusEvent(BookingStatus status, Guid changedByUserId, string? note)
    {
        Id = Guid.NewGuid();
        Status = status;
        ChangedByUserId = changedByUserId;
        Note = note;
        OccurredAt = DateTime.UtcNow;
    }
}