using Winche.Sentinel.Services;
using Microsoft.AspNetCore.Http;

namespace Winche.Sentinel.AspNetCore.Abstraction;

/// <summary>
/// An abstract implementation of <see cref="AsyncCallerClaimsAccessor{TResource}"/> that provides a method to set claims based on an <see cref="HttpContext"/>. 
/// </summary>
/// <typeparam name="TResource"></typeparam>
public abstract class HttpCallerClaimsAccessor<TResource> : AsyncCallerClaimsAccessor<TResource> where TResource : class
{
    /// <summary>
    /// Sets the claims for the current asynchronous context based on the provided <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> from which to extract claims.</param>
    public abstract void SetClaims(HttpContext httpContext);
}
