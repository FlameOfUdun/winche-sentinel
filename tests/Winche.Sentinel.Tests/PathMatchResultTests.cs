using Winche.Sentinel.Models;
using Xunit;

namespace Winche.Sentinel.Tests;

public class PathMatchResultTests
{
    [Fact]
    public void NoMatch_IsMatch_IsFalse()
    {
        Assert.False(PathMatchResult.NoMatch.IsMatch);
    }

    [Fact]
    public void NoMatch_Params_IsEmpty()
    {
        Assert.Empty(PathMatchResult.NoMatch.Params);
    }

    [Fact]
    public void NoMatch_IsSameInstanceEveryTime()
    {
        Assert.Same(PathMatchResult.NoMatch, PathMatchResult.NoMatch);
    }

    [Fact]
    public void Constructor_SetsIsMatch_True()
    {
        var result = new PathMatchResult(true, new Dictionary<string, string>());
        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Constructor_SetsIsMatch_False()
    {
        var result = new PathMatchResult(false, new Dictionary<string, string>());
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Constructor_SetsParams()
    {
        var @params = new Dictionary<string, string> { ["id"] = "42", ["org"] = "acme" };
        var result = new PathMatchResult(true, @params);
        Assert.Equal("42", result.Params["id"]);
        Assert.Equal("acme", result.Params["org"]);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var p = new Dictionary<string, string>();
        var a = new PathMatchResult(false, p);
        var b = new PathMatchResult(false, p);
        Assert.Equal(a, b);
    }
}
