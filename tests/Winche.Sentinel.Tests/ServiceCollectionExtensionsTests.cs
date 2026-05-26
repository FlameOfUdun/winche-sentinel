using Microsoft.Extensions.DependencyInjection;
using Winche.Sentinel.DependencyInjection;
using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Services;
using Xunit;

namespace Winche.Sentinel.Tests;

public class ServiceCollectionExtensionsTests
{
    private sealed class Resource { }

    private sealed class CustomClaimsAccessor : ICallerClaimsAccessor<Resource>
    {
        public IReadOnlyDictionary<string, object?> GetClaims() =>
            new Dictionary<string, object?> { ["custom"] = true };
    }

    [Fact]
    public void AddWincheSentinel_RegistersPathPatternMatcher()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var sp = services.BuildServiceProvider();

        var matcher = sp.GetService<IPathPatternMatcher<Resource>>();
        Assert.NotNull(matcher);
    }

    [Fact]
    public void AddWincheSentinel_PathPatternMatcherIsCorrectType()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var sp = services.BuildServiceProvider();

        var matcher = sp.GetRequiredService<IPathPatternMatcher<Resource>>();
        Assert.IsType<PathPatternMatcher<Resource>>(matcher);
    }

    [Fact]
    public void AddWincheSentinel_RegistersAccessRuleEvaluator()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var sp = services.BuildServiceProvider();

        var evaluator = sp.GetService<IAccessRuleEvaluator<Resource>>();
        Assert.NotNull(evaluator);
    }

    [Fact]
    public void AddWincheSentinel_AccessRuleEvaluatorIsCorrectType()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var sp = services.BuildServiceProvider();

        var evaluator = sp.GetRequiredService<IAccessRuleEvaluator<Resource>>();
        Assert.IsType<AccessRuleEvaluator<Resource>>(evaluator);
    }

    [Fact]
    public void AddWincheSentinel_RegistersEmptyCallerClaimsAccessorByDefault()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var sp = services.BuildServiceProvider();

        var accessor = sp.GetRequiredService<ICallerClaimsAccessor<Resource>>();
        Assert.IsType<EmptyClaimsAccessor<Resource>>(accessor);
    }

    [Fact]
    public void AddWincheSentinel_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddWincheSentinel<Resource>();
        Assert.Same(services, result);
    }

    [Fact]
    public void AddWincheSentinel_WithConfigure_InvokesConfigureAction()
    {
        var services = new ServiceCollection();
        var configureCalled = false;

        services.AddWincheSentinel<Resource>(_ => configureCalled = true);

        Assert.True(configureCalled);
    }

    [Fact]
    public void AddWincheSentinel_WithNullConfigure_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>(configure: null);
        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<IAccessRuleEvaluator<Resource>>());
    }

    [Fact]
    public void AddWincheSentinel_WithSetCallerClaimsAccessor_OverridesDefault()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>(cfg =>
            cfg.SetCallerClaimsAccessor<CustomClaimsAccessor>());
        var sp = services.BuildServiceProvider();

        // MS DI returns last registered when multiple are registered for same interface
        var accessor = sp.GetRequiredService<ICallerClaimsAccessor<Resource>>();
        Assert.IsType<CustomClaimsAccessor>(accessor);
    }

    [Fact]
    public void ConfigureWincheSentinel_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();

        var result = services.ConfigureWincheSentinel<Resource>(_ => { });

        Assert.Same(services, result);
    }

    [Fact]
    public void ConfigureWincheSentinel_InvokesConfigureAction()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        var configureCalled = false;

        services.ConfigureWincheSentinel<Resource>(_ => configureCalled = true);

        Assert.True(configureCalled);
    }

    [Fact]
    public void Services_CanBeRegisteredForMultipleResourceTypes_Independently()
    {
        var services = new ServiceCollection();
        services.AddWincheSentinel<Resource>();
        services.AddWincheSentinel<string>();
        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<IAccessRuleEvaluator<Resource>>());
        Assert.NotNull(sp.GetService<IAccessRuleEvaluator<string>>());
    }
}
