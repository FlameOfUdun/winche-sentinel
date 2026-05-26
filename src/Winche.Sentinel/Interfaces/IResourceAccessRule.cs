using Winche.Sentinel.Models;

namespace Winche.Sentinel.Interfaces;

/// <summary>
/// Defines an access rule for a specific resource type. 
/// An access rule consists of a path, a set of operations, and an evaluation function 
/// that determines whether access should be granted based on the provided context.
/// </summary>
/// <typeparam name="TResource">The type of the resource this access rule applies to.</typeparam>
public interface IResourceAccessRule<TResource> where TResource : class
{
    /// <summary>
    /// Gets the path that identifies the resource or resource pattern this access rule applies to.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the set of operations that this access rule applies to.
    /// </summary>
    IReadOnlySet<AccessOperation> Operations { get; }

    /// <summary>
    /// Evaluates the access rule against the provided context.
    /// </summary>
    /// <param name="context">The context in which to evaluate the access rule.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether access is granted.</returns>
    Task<bool> EvaluateAsync(AccessContext<TResource> context, CancellationToken ct);
}
