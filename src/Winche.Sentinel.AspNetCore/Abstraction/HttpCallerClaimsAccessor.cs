using Winche.Sentinel.Services;
using Microsoft.AspNetCore.Http;

namespace Winche.Sentinel.AspNetCore.Abstraction;

/// <summary>
/// An abstract implementation of <see cref="AsyncCallerClaimsAccessor{TResource}"/> that provides a method to set claims based on an <see cref="HttpContext"/>. 
/// </summary>
/// <typeparam name="TResource">The type of the resource associated with the claims.</typeparam>
public abstract class HttpCallerClaimsAccessor<TResource> : AsyncCallerClaimsAccessor<TResource> where TResource : class
{
    /// <summary>
    /// Sets the claims for the current asynchronous context based on the provided <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> from which to extract claims.</param>
    public virtual void SetClaims(HttpContext httpContext)
    {
        var claims = MapClaims(httpContext);
        SetClaims(claims);
    }

    /// <summary>
    /// Maps the claims from the provided <see cref="HttpContext"/> to a dictionary of claim types and values.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> from which to extract claims.</param>
    /// <returns>A dictionary of claim types and values.</returns>
    public abstract IReadOnlyDictionary<string, object?> MapClaims(HttpContext httpContext);
}
