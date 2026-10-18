using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Add_WithSameCurrency_SumsAmounts()
    {
        var a = new Money(100m, "ZAR");
        var b = new Money(50m, "ZAR");

        var result = a.Add(b);

        Assert.Equal(150m, result.Amount);
        Assert.Equal("ZAR", result.Currency);
    }

    [Fact]
    public void Subtract_WithSameCurrency_SubtractsAmounts()
    {
        var a = new Money(100m, "ZAR");
        var b = new Money(30m, "ZAR");

        var result = a.Subtract(b);

        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Multiply_ScalesAmount()
    {
        var money = new Money(200m, "ZAR");

        var result = money.Multiply(0.15m);

        Assert.Equal(30m, result.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        var zar = new Money(100m, "ZAR");
        var usd = new Money(100m, "USD");

        Assert.Throws<InvalidOperationException>(() => zar.Add(usd));
    }

    [Fact]
    public void Constructor_WithEmptyCurrency_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Money(100m, ""));
    }

    [Fact]
    public void Currency_IsNormalizedToUppercase()
    {
        var money = new Money(100m, "zar");

        Assert.Equal("ZAR", money.Currency);
    }
}