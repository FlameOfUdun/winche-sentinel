using Winche.Sentinel.Services;
using Xunit;

namespace Winche.Sentinel.Tests;

public class EmptyCallerClaimsAccessorTests
{
    private sealed class Resource { }

    private readonly EmptyClaimsAccessor<Resource> _sut = new();

    [Fact]
    public void GetClaims_ReturnsEmptyDictionary()
    {
        var claims = _sut.GetClaims();
        Assert.Empty(claims);
    }

    [Fact]
    public void GetClaims_ReturnsDictionaryOnEveryCall()
    {
        var claims1 = _sut.GetClaims();
        var claims2 = _sut.GetClaims();
        Assert.NotNull(claims1);
        Assert.NotNull(claims2);
    }

    [Fact]
    public void GetClaims_ReturnedDictionaryIsReadOnly()
    {
        var claims = _sut.GetClaims();
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(claims);
    }
}
