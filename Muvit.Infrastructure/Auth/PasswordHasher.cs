using Microsoft.AspNetCore.Identity;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.Entities;

namespace Muvit.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _identityHasher = new();

    // The Identity API's signature requires a TUser instance, but its implementation never
    // actually reads it — hashing/verification depends only on the password and a random salt
    // generated internally. Passing null! here is safe, and avoids forcing our own IPasswordHasher
    // interface to depend on a User instance that doesn't exist yet at registration time.
    public string HashPassword(string password)
        => _identityHasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string passwordHash)
        => _identityHasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}