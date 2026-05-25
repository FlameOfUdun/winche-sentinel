using WincheSentinel.Interfaces;
using WincheSentinel.Models;

namespace WincheSentinel.Services;

internal sealed class PathPatternMatcher<TResource> : IPathPatternMatcher<TResource> where TResource : class
{
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
