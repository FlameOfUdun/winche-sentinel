using WincheSentinel.Models;

namespace WincheSentinel.Interfaces;

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
    /// <param name="data">Optional data to use during evaluation.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EvaluateAsync(AccessOperation operation, string path, object? data = null, CancellationToken ct = default);
}
