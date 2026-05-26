namespace Winche.Sentinel.Interfaces;

/// <summary>
/// Defines an interface for accessing the caller claims in the context of evaluating access rules for a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource for which caller claims are being accessed.</typeparam>
public interface ICallerClaimsAccessor<TResource> where TResource : class
{
    /// <summary>
    /// Retrieves the caller claims as a read-only dictionary.
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<string, object?> GetClaims();
}
