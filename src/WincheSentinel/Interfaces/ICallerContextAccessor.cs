namespace WincheSentinel.Interfaces;

/// <summary>
/// Defines an interface for accessing caller context information, such as claims, for a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource this caller context accessor applies to.</typeparam>
public interface ICallerContextAccessor<TResource> where TResource : class
{
    /// <summary>
    /// Gets the claims associated with the caller context.
    /// </summary>
    /// <returns>A read-only dictionary of claims.</returns>
    IReadOnlyDictionary<string, object?> GetClaims();
}
