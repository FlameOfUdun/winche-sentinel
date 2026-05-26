using Winche.Sentinel.Models;
using Xunit;

namespace Winche.Sentinel.Tests;

public class AccessDeniedExceptionTests
{
    [Theory]
    [InlineData(AccessOperation.Read, "/docs/42", "Access denied: Read on '/docs/42'")]
    [InlineData(AccessOperation.Write, "/users/1", "Access denied: Write on '/users/1'")]
    [InlineData(AccessOperation.Delete, "/items/5", "Access denied: Delete on '/items/5'")]
    public void AccessDeniedException_Message_ContainsOperationAndPath(
        AccessOperation op, string path, string expected)
    {
        var ex = new AccessDeniedException(op, path);
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void AccessDeniedException_WithNullPath_FallsBackToResourceLiteral()
    {
        var ex = new AccessDeniedException(AccessOperation.Read, null!);
        Assert.Contains("resource", ex.Message);
    }

    [Fact]
    public void AccessDeniedException_IsException()
    {
        Assert.IsAssignableFrom<Exception>(new AccessDeniedException(AccessOperation.Read, "/x"));
    }

    [Theory]
    [InlineData(AccessOperation.Read, "/docs/42", "Access denied: Read on '/docs/42'")]
    [InlineData(AccessOperation.Write, "/users/1", "Access denied: Write on '/users/1'")]
    [InlineData(AccessOperation.Delete, "/items/5", "Access denied: Delete on '/items/5'")]
    public void NoRulesMatchedException_Message_ContainsOperationAndPath(
        AccessOperation op, string path, string expected)
    {
        var ex = new NoRulesMatchedException(op, path);
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void NoRulesMatchedException_WithNullPath_FallsBackToResourceLiteral()
    {
        var ex = new NoRulesMatchedException(AccessOperation.Delete, null!);
        Assert.Contains("resource", ex.Message);
    }

    [Fact]
    public void NoRulesMatchedException_IsException()
    {
        Assert.IsAssignableFrom<Exception>(new NoRulesMatchedException(AccessOperation.Read, "/x"));
    }

    [Fact]
    public void AccessDeniedException_And_NoRulesMatchedException_AreDistinctTypes()
    {
        Assert.NotEqual(
            typeof(AccessDeniedException),
            typeof(NoRulesMatchedException));
    }
}
