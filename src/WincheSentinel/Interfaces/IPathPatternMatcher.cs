using WincheSentinel.Models;

namespace WincheSentinel.Interfaces;

/// <summary>
/// Defines an interface for matching paths against patterns for a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource this path pattern matcher applies to.</typeparam>
public interface IPathPatternMatcher<TResource> where TResource : class
{
    /// <summary>
    /// Matches the specified path against the given pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match against.</param>
    /// <param name="path">The path to match.</param>
    /// <returns>A <see cref="PathMatchResult"/> indicating the result of the match.</returns>
    PathMatchResult Match(string pattern, string path);
}
