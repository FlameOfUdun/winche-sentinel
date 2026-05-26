using Xunit;
using Winche.Sentinel.Services;

namespace Winche.Sentinel.Tests;

public class PathPatternMatcherTests
{
    private readonly PathPatternMatcher<object> _sut = new();

    // ── Exact match ──────────────────────────────────────────────────────────

    [Fact]
    public void Exact_matching_segment_matches()
    {
        var result = _sut.Match("/docs/42", "/docs/42");
        Assert.True(result.IsMatch);
        Assert.Empty(result.Params);
    }

    [Fact]
    public void Exact_different_segment_does_not_match()
    {
        var result = _sut.Match("/docs/42", "/docs/43");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Exact_match_is_case_sensitive()
    {
        var result = _sut.Match("/docs/42", "/Docs/42");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Exact_pattern_does_not_match_longer_path()
    {
        var result = _sut.Match("/docs/42", "/docs/42/extra");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Exact_pattern_does_not_match_shorter_path()
    {
        var result = _sut.Match("/docs/42/extra", "/docs/42");
        Assert.False(result.IsMatch);
    }

    // ── Named params {id} ────────────────────────────────────────────────────

    [Fact]
    public void Named_param_captures_segment()
    {
        var result = _sut.Match("/docs/{id}", "/docs/42");
        Assert.True(result.IsMatch);
        Assert.Equal("42", result.Params["id"]);
    }

    [Fact]
    public void Named_param_does_not_match_extra_segments()
    {
        var result = _sut.Match("/docs/{id}", "/docs/42/extra");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Multiple_named_params_all_captured()
    {
        var result = _sut.Match("/orgs/{org}/repos/{repo}", "/orgs/acme/repos/sentinel");
        Assert.True(result.IsMatch);
        Assert.Equal("acme", result.Params["org"]);
        Assert.Equal("sentinel", result.Params["repo"]);
    }

    [Fact]
    public void Named_param_does_not_match_empty_path()
    {
        var result = _sut.Match("/docs/{id}", "");
        Assert.False(result.IsMatch);
    }

    // ── Single wildcard * ────────────────────────────────────────────────────

    [Fact]
    public void Wildcard_matches_any_single_segment()
    {
        var result = _sut.Match("/docs/*", "/docs/anything");
        Assert.True(result.IsMatch);
        Assert.Empty(result.Params);
    }

    [Fact]
    public void Wildcard_does_not_match_multiple_segments()
    {
        var result = _sut.Match("/docs/*", "/docs/a/b");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Wildcard_in_middle_matches_correct_path()
    {
        var result = _sut.Match("/docs/*/comments", "/docs/42/comments");
        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Wildcard_in_middle_does_not_match_wrong_trailing_segment()
    {
        var result = _sut.Match("/docs/*/comments", "/docs/42/other");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Multiple_wildcards_match_independently()
    {
        var result = _sut.Match("/orgs/*/repos/*/issues", "/orgs/acme/repos/sentinel/issues");
        Assert.True(result.IsMatch);
    }

    // ── Double wildcard ** ───────────────────────────────────────────────────

    [Fact]
    public void Double_wildcard_captures_single_remaining_segment()
    {
        var result = _sut.Match("/docs/**", "/docs/42");
        Assert.True(result.IsMatch);
        Assert.Equal("42", result.Params["**"]);
    }

    [Fact]
    public void Double_wildcard_captures_multiple_remaining_segments()
    {
        var result = _sut.Match("/docs/**", "/docs/a/b/c");
        Assert.True(result.IsMatch);
        Assert.Equal("a/b/c", result.Params["**"]);
    }

    [Fact]
    public void Double_wildcard_matches_zero_remaining_segments()
    {
        var result = _sut.Match("/docs/**", "/docs");
        Assert.True(result.IsMatch);
        Assert.Equal("", result.Params["**"]);
    }

    [Fact]
    public void Double_wildcard_alone_matches_any_path()
    {
        var result = _sut.Match("/**", "/foo/bar/baz");
        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Double_wildcard_alone_matches_empty_path()
    {
        var result = _sut.Match("/**", "");
        Assert.True(result.IsMatch);
        Assert.Equal("", result.Params["**"]);
    }

    [Fact]
    public void Named_param_before_double_wildcard_both_captured()
    {
        var result = _sut.Match("/orgs/{org}/**", "/orgs/acme/repos/sentinel");
        Assert.True(result.IsMatch);
        Assert.Equal("acme", result.Params["org"]);
        Assert.Equal("repos/sentinel", result.Params["**"]);
    }

    // ── Empty / root patterns ────────────────────────────────────────────────

    [Fact]
    public void Empty_pattern_never_matches()
    {
        var result = _sut.Match("", "/docs/42");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Root_slash_pattern_never_matches()
    {
        var result = _sut.Match("/", "/docs/42");
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Leading_and_trailing_slashes_are_normalised()
    {
        var result = _sut.Match("/docs/42/", "/docs/42");
        Assert.True(result.IsMatch);
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Double_wildcard_not_last_throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Match("/a/**/b", "/a/x/b"));
    }

    [Fact]
    public void Empty_param_name_throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Match("/docs/{}", "/docs/42"));
    }
}
