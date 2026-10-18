using Muvit.Domain.Enums;

namespace Muvit.Domain.Entities;

public abstract class User
{
    public Guid Id { get; private init; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }

    protected User(string fullName, string email, string phoneNumber, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        Id = Guid.NewGuid();
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public abstract UserRole Role { get; }

    public void Suspend()
    {
        if (Status == UserStatus.Banned)
            throw new InvalidOperationException("A banned user cannot be suspended — they are already permanently blocked.");

        Status = UserStatus.Suspended;
    }

    public void Ban() => Status = UserStatus.Banned;

    public void Reactivate()
    {
        if (Status == UserStatus.Banned)
            throw new InvalidOperationException("A banned user cannot be reactivated.");

        Status = UserStatus.Active;
    }

    public void UpdateContactDetails(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        Email = email;
        PhoneNumber = phoneNumber;
    }
}