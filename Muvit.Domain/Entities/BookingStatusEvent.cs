using Muvit.Domain.Enums;

namespace Muvit.Domain.Entities;

public sealed class BookingStatusEvent
{
    public Guid Id { get; }
    public BookingStatus Status { get; private set;}
    public Guid ChangedByUserId { get; private set;}
    public string? Note { get; private set;}
    public DateTime OccurredAt { get; private set;}
    private BookingStatusEvent() { }

    internal BookingStatusEvent(BookingStatus status, Guid changedByUserId, string? note)
    {
        Id = Guid.NewGuid();
        Status = status;
        ChangedByUserId = changedByUserId;
        Note = note;
        OccurredAt = DateTime.UtcNow;
    }
}