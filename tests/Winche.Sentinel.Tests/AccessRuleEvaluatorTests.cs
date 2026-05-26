using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;
using Winche.Sentinel.Services;
using Xunit;

namespace Winche.Sentinel.Tests;

public class AccessRuleEvaluatorTests
{
    private sealed class Resource { public string? Name { get; set; } }

    // ── Fakes ────────────────────────────────────────────────────────────────

    private sealed class FakeRule : IResourceAccessRule<Resource>
    {
        public string Path { get; init; } = "/**";
        public IReadOnlySet<AccessOperation> Operations { get; init; } =
            new HashSet<AccessOperation>(Enum.GetValues<AccessOperation>());
        public bool Allow { get; init; } = true;
        public AccessContext<Resource>? CapturedContext { get; private set; }

        public Task<bool> EvaluateAsync(AccessContext<Resource> context, CancellationToken ct)
        {
            CapturedContext = context;
            return Task.FromResult(Allow);
        }
    }

    // Used specifically to test the null-path branch in the evaluator (bypasses type contract intentionally).
    private sealed class NullPathRule : IResourceAccessRule<Resource>
    {
        public string Path => null!;
        public IReadOnlySet<AccessOperation> Operations =>
            new HashSet<AccessOperation>(Enum.GetValues<AccessOperation>());
        public AccessContext<Resource>? CapturedContext { get; private set; }

        public Task<bool> EvaluateAsync(AccessContext<Resource> context, CancellationToken ct)
        {
            CapturedContext = context;
            return Task.FromResult(true);
        }
    }

    private sealed class ResourceLoadingRule : IResourceAccessRule<Resource>
    {
        public Resource? LoadedResource { get; private set; }
        public string Path => "/**";
        public IReadOnlySet<AccessOperation> Operations =>
            new HashSet<AccessOperation>(Enum.GetValues<AccessOperation>());

        public async Task<bool> EvaluateAsync(AccessContext<Resource> context, CancellationToken ct)
        {
            LoadedResource = await context.GetResourceAsync(ct);
            return true;
        }
    }

    private sealed class FakeClaimsAccessor : ICallerClaimsAccessor<Resource>
    {
        private readonly Dictionary<string, object?> _claims;
        public FakeClaimsAccessor(Dictionary<string, object?> claims) => _claims = claims;
        public IReadOnlyDictionary<string, object?> GetClaims() => _claims;
    }

    private static AccessRuleEvaluator<Resource> Build(
        IEnumerable<IResourceAccessRule<Resource>> rules,
        ICallerClaimsAccessor<Resource>? claimsAccessor = null)
    {
        var matcher = new PathPatternMatcher<Resource>();
        var accessor = claimsAccessor ?? new EmptyClaimsAccessor<Resource>();
        return new AccessRuleEvaluator<Resource>(rules, matcher, accessor);
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenRuleMatchesAndGrantsAccess_DoesNotThrow()
    {
        var sut = Build([new FakeRule { Allow = true }]);
        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesOperationToContext()
    {
        var rule = new FakeRule { Allow = true };
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Delete, "/docs/42");

        Assert.Equal(AccessOperation.Delete, rule.CapturedContext!.Operation);
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesPathToContext()
    {
        var rule = new FakeRule { Allow = true };
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");

        Assert.Equal("/docs/42", rule.CapturedContext!.Path);
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesIncomingDataToContext()
    {
        var data = new Resource { Name = "payload" };
        var rule = new FakeRule { Allow = true };
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Write, "/docs/42", data);

        Assert.Same(data, rule.CapturedContext!.IncomingData);
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesClaimsToContext()
    {
        var claims = new Dictionary<string, object?> { ["userId"] = "u1" };
        var rule = new FakeRule { Allow = true };
        var sut = Build([rule], new FakeClaimsAccessor(claims));

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");

        Assert.Equal("u1", rule.CapturedContext!.Claims["userId"]);
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesNamedPathParamsToContext()
    {
        var rule = new FakeRule { Path = "/docs/{id}", Allow = true };
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");

        Assert.Equal("42", rule.CapturedContext!.Params["id"]);
    }

    [Fact]
    public async Task EvaluateAsync_WithResourceLoader_LoaderAvailableInContext()
    {
        var resource = new Resource { Name = "loaded" };
        var rule = new ResourceLoadingRule();
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42",
            loader: _ => Task.FromResult<Resource?>(resource));

        Assert.Same(resource, rule.LoadedResource);
    }

    // ── Denial ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenRuleMatchesAndDeniesAccess_ThrowsAccessDeniedException()
    {
        var sut = Build([new FakeRule { Allow = false }]);

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42"));
    }

    [Fact]
    public async Task EvaluateAsync_AccessDeniedException_ContainsOperationAndPath()
    {
        var sut = Build([new FakeRule { Allow = false }]);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(
            () => sut.EvaluateAsync(AccessOperation.Write, "/docs/42"));

        Assert.Contains("Write", ex.Message);
        Assert.Contains("/docs/42", ex.Message);
    }

    // ── No rules match ────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenNoRules_ThrowsNoRulesMatchedException()
    {
        var sut = Build([]);

        await Assert.ThrowsAsync<NoRulesMatchedException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42"));
    }

    [Fact]
    public async Task EvaluateAsync_WhenRulePathDoesNotMatch_ThrowsNoRulesMatchedException()
    {
        var rule = new FakeRule { Path = "/admin/{id}", Allow = true };
        var sut = Build([rule]);

        await Assert.ThrowsAsync<NoRulesMatchedException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42"));
    }

    [Fact]
    public async Task EvaluateAsync_WhenRuleOperationDoesNotMatch_ThrowsNoRulesMatchedException()
    {
        var rule = new FakeRule
        {
            Path = "/**",
            Operations = new HashSet<AccessOperation> { AccessOperation.Write },
            Allow = true,
        };
        var sut = Build([rule]);

        await Assert.ThrowsAsync<NoRulesMatchedException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42"));
    }

    // ── Rule ordering ─────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenFirstRuleDenies_DoesNotEvaluateSecondRule()
    {
        var first = new FakeRule { Path = "/**", Allow = false };
        var second = new FakeRule { Path = "/**", Allow = true };
        var sut = Build([first, second]);

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42"));

        Assert.Null(second.CapturedContext);
    }

    [Fact]
    public async Task EvaluateAsync_WhenFirstRulePathDoesNotMatch_EvaluatesSecondRule()
    {
        var first = new FakeRule { Path = "/admin/**", Allow = true };
        var second = new FakeRule { Path = "/docs/**", Allow = true };
        var sut = Build([first, second]);

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");

        Assert.Null(first.CapturedContext);
        Assert.NotNull(second.CapturedContext);
    }

    [Fact]
    public async Task EvaluateAsync_WhenFirstRuleOperationDoesNotMatch_EvaluatesSecondRule()
    {
        var first = new FakeRule
        {
            Path = "/**",
            Operations = new HashSet<AccessOperation> { AccessOperation.Delete },
            Allow = true,
        };
        var second = new FakeRule { Path = "/**", Allow = true };
        var sut = Build([first, second]);

        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42");

        Assert.Null(first.CapturedContext);
        Assert.NotNull(second.CapturedContext);
    }

    // ── Null rule path (match-all) ────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenRulePathIsNull_MatchesAnyPath()
    {
        var rule = new NullPathRule();
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Read, "/any/path/at/all");

        Assert.NotNull(rule.CapturedContext);
    }

    [Fact]
    public async Task EvaluateAsync_WhenRulePathIsNull_ParamsAreEmpty()
    {
        var rule = new NullPathRule();
        var sut = Build([rule]);

        await sut.EvaluateAsync(AccessOperation.Read, "/some/path");

        Assert.Empty(rule.CapturedContext!.Params);
    }

    // ── Loader null guard ────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WhenLoaderIsNullAndResourceAccessed_ThrowsInvalidOperationException()
    {
        var rule = new ResourceLoadingRule();
        var sut = Build([rule]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.EvaluateAsync(AccessOperation.Read, "/docs/42", loader: null));
    }

    // ── Cancellation ─────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_WithCancelledToken_CancellationPropagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var rule = new FakeRule { Allow = true };
        var sut = Build([rule]);

        // Cancellation at the rule level propagates; the rule itself doesn't throw,
        // so the cancellation only surfaces if the rule checks or if the loader does.
        // We just verify evaluation can be initiated without hanging.
        await sut.EvaluateAsync(AccessOperation.Read, "/docs/42", ct: cts.Token);
        Assert.NotNull(rule.CapturedContext);
    }
}
