using Muvit.Domain.Entities;

namespace Muvit.Application.Interfaces.Services;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}