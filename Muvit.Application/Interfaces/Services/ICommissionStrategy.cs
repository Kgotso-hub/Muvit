using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Interfaces.Services;

public interface ICommissionStrategy
{
    decimal GetCommissionRate(Money grossAmount);
}