using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;

namespace Winche.Sentinel.Services;

/// <summary>
/// Matches paths against patterns with support for named parameters and wildcards.
/// </summary>
/// <typeparam name="TResource"></typeparam>
internal sealed class PathPatternMatcher<TResource> : IPathPatternMatcher<TResource> where TResource : class
{
    /// <summary>
    /// Matches a path against a pattern and captures parameters if the pattern matches.
    /// </summary>
    /// <param name="pattern">The pattern to match against.</param>
    /// <param name="path">The path to be matched.</param>
    /// <returns>A <see cref="PathMatchResult"/> indicating whether the match was successful and any captured parameters.</returns>
    /// <exception cref="ArgumentException">Thrown when the pattern is invalid.</exception>
    public PathMatchResult Match(string pattern, string path)
    {
        var patternSegments = pattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (patternSegments.Length == 0)
            return PathMatchResult.NoMatch;

        if (pathSegments.Length == 0)
            return patternSegments is ["**"]
                ? PathMatchResult.Match(new Dictionary<string, string> { ["**"] = "" })
                : PathMatchResult.NoMatch;

        var captured = new Dictionary<string, string>();
        var pi = 0;

        for (var i = 0; i < patternSegments.Length; i++)
        {
            var seg = patternSegments[i];

            if (seg == "**")
            {
                if (i != patternSegments.Length - 1)
                    throw new ArgumentException("'**' must be the last segment in the pattern.");

                captured["**"] = string.Join("/", pathSegments[pi..]);

                return PathMatchResult.Match(captured);
            }

            if (pi >= pathSegments.Length)
                return PathMatchResult.NoMatch;

            if (seg[0] == '{' && seg[^1] == '}')
            {
                if (seg.Length == 2)
                    throw new ArgumentException($"Empty parameter name '{{}}' is not valid in path pattern.");
                captured[seg[1..^1]] = pathSegments[pi];
            }
            else if (seg != "*" && !string.Equals(seg, pathSegments[pi], StringComparison.Ordinal))
            {
                return PathMatchResult.NoMatch;
            }

            pi++;
        }

        if (pi != pathSegments.Length)
            return PathMatchResult.NoMatch;

        return PathMatchResult.Match(captured);
    }
}
