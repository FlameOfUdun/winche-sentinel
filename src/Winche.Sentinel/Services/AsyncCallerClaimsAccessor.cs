using System.Collections.Immutable;
using Winche.Sentinel.Interfaces;

namespace Winche.Sentinel.Services;

/// <summary>
/// An implementation of <see cref="ICallerClaimsAccessor{TResource}"/> that uses <see cref="AsyncLocal{T}"/> to store claims for the current asynchronous context. 
/// This allows claims to be set and accessed across asynchronous calls without losing the context. The claims are stored as a dictionary of string keys and object values, and can be set at the beginning of a request or operation to establish the claims for that context.
/// </summary>
/// <typeparam name="TResource">The type of the resource associated with the claims.</typeparam>
public class AsyncCallerClaimsAccessor<TResource> : ICallerClaimsAccessor<TResource> where TResource : class
{
    private readonly AsyncLocal<IReadOnlyDictionary<string, object?>> _asyncLocal = new();

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> GetClaims()
    {
        return _asyncLocal.Value ??= ImmutableDictionary<string, object?>.Empty;
    }

    /// <summary>
    /// Sets the claims for the current asynchronous context. This method should be called at the beginning of a request or operation to establish the claims for that context.
    /// </summary>
    /// <param name="claims">The claims to set for the current asynchronous context.</param>
    public void SetClaims(Dictionary<string, object?> claims)
    {
        _asyncLocal.Value = claims;
    }

    /// <summary>
    /// Sets the claims for the current asynchronous context. This method should be called at the beginning of a request or operation to establish the claims for that context.
    /// </summary>
    /// <param name="claims">The claims to set for the current asynchronous context.</param>
    public void SetClaims(IReadOnlyDictionary<string, object?> claims)
    {
        _asyncLocal.Value = claims;
    }
}
