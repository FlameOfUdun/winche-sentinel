using System.Collections.Immutable;

namespace WincheSentinel.Models;

/// <summary>
/// Represents the result of matching a resource path against a defined path pattern, indicating whether the path matches the pattern and providing any extracted parameters from the path if a match is successful. This record is used in the path pattern matching process to determine if a given resource path matches a defined pattern and to extract any relevant parameters that may be needed for access evaluation or other processing based on the matched path pattern.
/// </summary>
/// <param name="IsMatch">Indicates whether the resource path matches the defined pattern.</param>
/// <param name="Params">A read-only dictionary of parameters extracted from the resource path if a match is successful.</param>
public sealed record PathMatchResult(bool IsMatch, IReadOnlyDictionary<string, string> Params)
{
    /// <summary>
    /// Gets a static instance of <see cref="PathMatchResult"/> representing a failed match, where the resource path does not match the defined pattern. The <see cref="IsMatch"/> property is set to false, and the <see cref="Params"/> property is an empty dictionary. This instance can be used as a default value to indicate that no match was found when attempting to match a resource path against a defined pattern.
    /// </summary>
    public static PathMatchResult NoMatch { get; } = new(false, ImmutableDictionary<string, string>.Empty);

    /// <summary>
    /// Creates a new instance of <see cref="PathMatchResult"/> representing a successful match, where the resource path matches the defined pattern. The <see cref="IsMatch"/> property is set to true, and the <see cref="Params"/> property contains the provided dictionary of parameters extracted from the resource path. This method can be used to create a successful match result when a resource path successfully matches a defined pattern and relevant parameters are extracted for further processing.
    /// </summary>
    /// <param name="params">A read-only dictionary of parameters extracted from the resource path.</param>
    /// <returns>A <see cref="PathMatchResult"/> instance representing a successful match.</returns>
    internal static PathMatchResult Match(IReadOnlyDictionary<string, string> @params) => new(true, @params);
}
