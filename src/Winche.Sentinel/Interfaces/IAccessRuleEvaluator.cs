using Winche.Sentinel.Models;

namespace Winche.Sentinel.Interfaces;

/// <summary>
/// Defines an evaluator for access rules of a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource this evaluator applies to.</typeparam>
public interface IAccessRuleEvaluator<TResource> where TResource : class
{
    /// <summary>
    /// Evaluates the access rules for the specified operation and path.
    /// </summary>
    /// <param name="operation">The operation to evaluate.</param>
    /// <param name="path">The path of the resource.</param>
    /// <param name="data">Optional incoming data to use during evaluation.</param>
    /// <param name="loader">Function to load the resource.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EvaluateAsync(AccessOperation operation, string path, object? data = null, Func<CancellationToken, Task<TResource?>>? loader = null, CancellationToken ct = default);
}
