using Winche.Sentinel.Interfaces;

namespace Winche.Sentinel.Services;

/// <summary>
/// An implementation of <see cref="ICallerClaimsAccessor{TResource}"/> that returns an empty set of claims.
/// </summary>
/// <typeparam name="TResource">The type of the resource for which to return claims.</typeparam>
public sealed class EmptyClaimsAccessor<TResource> : ICallerClaimsAccessor<TResource> where TResource : class
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> GetClaims()
    {
        return new Dictionary<string, object?>();
    }
}
