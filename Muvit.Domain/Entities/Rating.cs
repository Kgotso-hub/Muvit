namespace Muvit.Domain.Entities;

public sealed class Rating
{
    public Guid Id { get; }
    public Guid BookingId { get; }
    public Guid RaterUserId { get; }
    public Guid RateeUserId { get; }
    public int Value { get; }
    public string? Comment { get; }
    public DateTime CreatedAt { get; }

    public Rating(Guid bookingId, Guid raterUserId, Guid rateeUserId, int value, string? comment = null)
    {
        if (raterUserId == rateeUserId)
            throw new ArgumentException("A user cannot rate themselves.", nameof(rateeUserId));
        if (value < 1 || value > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        RaterUserId = raterUserId;
        RateeUserId = rateeUserId;
        Value = value;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}