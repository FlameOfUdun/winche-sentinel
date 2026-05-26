using Microsoft.Extensions.DependencyInjection;
using Winche.Sentinel.DependencyInjection;
using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;
using Xunit;

namespace Winche.Sentinel.Tests;

public class DependencyConfiguratorTests
{
    private sealed class Resource { }

    private sealed class RuleA : IResourceAccessRule<Resource>
    {
        public string Path => "/**";
        public IReadOnlySet<AccessOperation> Operations =>
            new HashSet<AccessOperation>(Enum.GetValues<AccessOperation>());
        public Task<bool> EvaluateAsync(AccessContext<Resource> context, CancellationToken ct) =>
            Task.FromResult(true);
    }

    private sealed class RuleB : IResourceAccessRule<Resource>
    {
        public string Path => "/b/**";
        public IReadOnlySet<AccessOperation> Operations =>
            new HashSet<AccessOperation>(Enum.GetValues<AccessOperation>());
        public Task<bool> EvaluateAsync(AccessContext<Resource> context, CancellationToken ct) =>
            Task.FromResult(true);
    }

    private sealed class ClaimsAccessorA : ICallerClaimsAccessor<Resource>
    {
        public IReadOnlyDictionary<string, object?> GetClaims() =>
            new Dictionary<string, object?> { ["source"] = "A" };
    }

    [Fact]
    public void AddResourceAccessRule_ByType_RegistersRuleInServiceCollection()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        cfg.AddResourceAccessRule<RuleA>();

        var sp = services.BuildServiceProvider();
        var rules = sp.GetServices<IResourceAccessRule<Resource>>();
        Assert.Single(rules);
        Assert.IsType<RuleA>(rules.First());
    }

    [Fact]
    public void AddResourceAccessRule_ByInstance_RegistersInstanceInServiceCollection()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);
        var instance = new RuleA();

        cfg.AddResourceAccessRule(instance);

        var sp = services.BuildServiceProvider();
        var rules = sp.GetServices<IResourceAccessRule<Resource>>();
        Assert.Single(rules);
        Assert.Same(instance, rules.First());
    }

    [Fact]
    public void AddResourceAccessRule_MultipleRules_AllRegistered()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        cfg.AddResourceAccessRule<RuleA>()
           .AddResourceAccessRule<RuleB>();

        var sp = services.BuildServiceProvider();
        var rules = sp.GetServices<IResourceAccessRule<Resource>>().ToList();
        Assert.Equal(2, rules.Count);
        Assert.Contains(rules, r => r is RuleA);
        Assert.Contains(rules, r => r is RuleB);
    }

    [Fact]
    public void SetCallerClaimsAccessor_ByType_RegistersAccessorInServiceCollection()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        cfg.SetCallerClaimsAccessor<ClaimsAccessorA>();

        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<ICallerClaimsAccessor<Resource>>();
        Assert.IsType<ClaimsAccessorA>(accessor);
    }

    [Fact]
    public void SetCallerClaimsAccessor_ByInstance_RegistersInstanceInServiceCollection()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);
        var instance = new ClaimsAccessorA();

        cfg.SetCallerClaimsAccessor<ClaimsAccessorA>(instance);

        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<ICallerClaimsAccessor<Resource>>();
        Assert.Same(instance, accessor);
    }

    [Fact]
    public void AddResourceAccessRule_ByType_ReturnsSameConfigurator()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        var result = cfg.AddResourceAccessRule<RuleA>();

        Assert.Same(cfg, result);
    }

    [Fact]
    public void AddResourceAccessRule_ByInstance_ReturnsSameConfigurator()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        var result = cfg.AddResourceAccessRule(new RuleA());

        Assert.Same(cfg, result);
    }

    [Fact]
    public void SetCallerClaimsAccessor_ByType_ReturnsSameConfigurator()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        var result = cfg.SetCallerClaimsAccessor<ClaimsAccessorA>();

        Assert.Same(cfg, result);
    }

    [Fact]
    public void SetCallerClaimsAccessor_ByInstance_ReturnsSameConfigurator()
    {
        var services = new ServiceCollection();
        var cfg = new DependencyConfigurator<Resource>(services);

        var result = cfg.SetCallerClaimsAccessor<ClaimsAccessorA>(new ClaimsAccessorA());

        Assert.Same(cfg, result);
    }
}
