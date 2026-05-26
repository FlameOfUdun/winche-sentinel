using Winche.Sentinel.Models;
using Xunit;

namespace Winche.Sentinel.Tests;

public class AccessContextTests
{
    private sealed class Resource { }

    [Fact]
    public void GetIncomingData_WhenDataIsExpectedType_ReturnsData()
    {
        var incoming = new Resource();
        var ctx = MakeContext(incoming);
        Assert.Same(incoming, ctx.GetIncomingData<Resource>());
    }

    [Fact]
    public void GetIncomingData_WhenDataIsWrongType_ReturnsDefault()
    {
        var ctx = MakeContext("not a resource");
        Assert.Null(ctx.GetIncomingData<Resource>());
    }

    [Fact]
    public void GetIncomingData_WhenDataIsNull_ReturnsDefault()
    {
        var ctx = MakeContext(null);
        Assert.Null(ctx.GetIncomingData<Resource>());
    }

    [Fact]
    public void GetIncomingData_WithValueType_ReturnsDefault()
    {
        var ctx = MakeContext(null);
        Assert.Equal(default, ctx.GetIncomingData<int>());
    }

    [Fact]
    public void Claims_DefaultValue_IsEmptyDictionary()
    {
        var ctx = new AccessContext<Resource>
        {
            Operation = AccessOperation.Read,
            Path = "/test",
            GetResourceAsync = _ => Task.FromResult<Resource?>(null),
        };
        Assert.Empty(ctx.Claims);
    }

    [Fact]
    public void Params_DefaultValue_IsEmptyDictionary()
    {
        var ctx = new AccessContext<Resource>
        {
            Operation = AccessOperation.Read,
            Path = "/test",
            GetResourceAsync = _ => Task.FromResult<Resource?>(null),
        };
        Assert.Empty(ctx.Params);
    }

    [Fact]
    public void IncomingData_DefaultValue_IsNull()
    {
        var ctx = new AccessContext<Resource>
        {
            Operation = AccessOperation.Read,
            Path = "/test",
            GetResourceAsync = _ => Task.FromResult<Resource?>(null),
        };
        Assert.Null(ctx.IncomingData);
    }

    [Fact]
    public void WithExpression_CreatesNewRecord_WithUpdatedParams()
    {
        var ctx = new AccessContext<Resource>
        {
            Operation = AccessOperation.Read,
            Path = "/test",
            GetResourceAsync = _ => Task.FromResult<Resource?>(null),
        };
        var updated = ctx with { Params = new Dictionary<string, string> { ["id"] = "42" } };
        Assert.Equal("42", updated.Params["id"]);
        Assert.Empty(ctx.Params);
    }

    private static AccessContext<Resource> MakeContext(object? incoming) => new()
    {
        Operation = AccessOperation.Read,
        Path = "/test",
        IncomingData = incoming,
        GetResourceAsync = _ => Task.FromResult<Resource?>(null),
    };
}
