using Muvit.Domain.Enums;

namespace Muvit.Domain.Entities;

public sealed class Admin : User
{
    public Admin(string fullName, string email, string phoneNumber, string passwordHash)
        : base(fullName, email, phoneNumber, passwordHash)
    {
    }

    public override UserRole Role => UserRole.Admin;
}