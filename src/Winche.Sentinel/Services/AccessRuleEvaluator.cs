using System.Collections.Immutable;
using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;

namespace Winche.Sentinel.Services;

/// <summary>
/// Implements the <see cref="IAccessRuleEvaluator{TResource}"/> interface to evaluate access rules for a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource for which access rules are being evaluated.</typeparam>
/// <param name="accessRules">A collection of access rules to be evaluated for the specified resource type.</param>
/// <param name="pathMatcher">An instance of <see cref="IPathPatternMatcher{TResource}"/> used to match resource paths against defined patterns in the access rules.</param>
/// <param name="claimsAccessor">An instance of <see cref="ICallerClaimsAccessor{TResource}"/> used to retrieve the caller claims for access evaluation.</param>
public sealed class AccessRuleEvaluator<TResource>(
    IEnumerable<IResourceAccessRule<TResource>> accessRules,
    IPathPatternMatcher<TResource> pathMatcher,
    ICallerClaimsAccessor<TResource> claimsAccessor
) : IAccessRuleEvaluator<TResource> where TResource : class
{
    /// <summary>
    /// Evaluates the access rules for a given operation, resource path, and optional data.
    /// </summary>
    /// <param name="operation">The type of access operation being performed on the resource.</param>
    /// <param name="path">The path of the resource being accessed.</param>
    /// <param name="data">Optional incoming write data.</param>
    /// <param name="loader">Resource loader function.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task EvaluateAsync(AccessOperation operation, string path,  object? data = null, Func<CancellationToken, Task<TResource?>>? loader = null, CancellationToken ct = default)
    {
        var context = new AccessContext<TResource>
        {
            Operation = operation,
            Path = path,
            IncomingData = data,
            Claims = claimsAccessor.GetClaims(),
            GetResourceAsync = (ct) =>
            {
                if (loader is null)
                    throw new InvalidOperationException("Resource loader function must be provided for operations that require resource access.");

                return loader(ct);
            },
        };

        await EvaluateRulesAsync(context, operation, path, ct);
    }

    /// <summary>
    /// OR semantics: a request is granted if ANY rule whose path pattern and
    /// operation set match returns <c>true</c>; a matching rule that returns <c>false</c> does not
    /// veto, it simply does not grant. Access is the default-deny outcome:
    /// <see cref="AccessDeniedException"/> when rules matched but none granted, and
    /// <see cref="NoRulesMatchedException"/> when no rule matched the path and operation at all.
    /// Registration order does not affect the decision.
    /// </summary>
    private async Task EvaluateRulesAsync(AccessContext<TResource> context, AccessOperation operation, string path, CancellationToken ct)
    {
        var anyRuleMatched = false;

        foreach (var rule in accessRules)
        {
            if (rule.Operations is not null && !rule.Operations.Contains(operation))
                continue;

            IReadOnlyDictionary<string, string> pathParams;

            if (rule.Path is null)
            {
                pathParams = ImmutableDictionary<string, string>.Empty;
            }
            else
            {
                var result = pathMatcher.Match(rule.Path, path);
                if (!result.IsMatch)
                    continue;

                pathParams = result.Params;
            }

            anyRuleMatched = true;

            if (await rule.EvaluateAsync(context with { Params = pathParams }, ct))
                return;
        }

        if (anyRuleMatched)
            throw new AccessDeniedException(operation, path);

        throw new NoRulesMatchedException(operation, path);
    }
}
