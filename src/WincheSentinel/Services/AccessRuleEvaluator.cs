using System.Collections.Immutable;
using WincheSentinel.Interfaces;
using WincheSentinel.Models;

namespace WincheSentinel.Services;

/// <summary>
/// Implements the <see cref="IAccessRuleEvaluator{TResource}"/> interface to evaluate access rules for a specific resource type. This class takes a collection of access rules, a path pattern matcher, a caller context accessor, and a resource object accessor as dependencies to perform access evaluation based on the defined rules and the context of the access request. The evaluation process involves matching the resource path against the defined rules, checking if the operation is allowed by the rule, and evaluating the rule's logic to determine whether access should be granted or denied. If no rules match or if access is denied by any rule, appropriate exceptions are thrown to indicate the outcome of the evaluation process.
/// </summary>
/// <typeparam name="TResource">The type of the resource for which access rules are being evaluated.</typeparam>
/// <param name="accessRules">A collection of access rules to be evaluated for the specified resource type.</param>
/// <param name="pathMatcher">An instance of <see cref="IPathPatternMatcher{TResource}"/> used to match resource paths against defined patterns in the access rules.</param>
/// <param name="contextAccessor">An instance of <see cref="ICallerContextAccessor{TResource}"/> used to retrieve the caller context for access evaluation.</param>
/// <param name="resourceAccessor">An instance of <see cref="IResourceObjectAccessor{TResource}"/> used to retrieve the resource object for access evaluation.</param>
public sealed class AccessRuleEvaluator<TResource>(
    IEnumerable<IResourceAccessRule<TResource>> accessRules,
    IPathPatternMatcher<TResource> pathMatcher, 
    ICallerContextAccessor<TResource> contextAccessor,
    IResourceObjectAccessor<TResource> resourceAccessor
) : IAccessRuleEvaluator<TResource> where TResource : class
{
    /// <summary>
    /// Evaluates the access rules for a given operation, resource path, and optional data. This method builds the access context based on the provided parameters and then evaluates the defined access rules against the context. If a rule matches and grants access, the method returns successfully. If a rule matches but denies access, an <see cref="AccessDeniedException"/> is thrown. If no rules match the provided operation and path, a <see cref="NoRulesMatchedException"/> is thrown to indicate that no applicable rules were found for the access request.
    /// </summary>
    /// <param name="operation">The type of access operation being performed on the resource.</param>
    /// <param name="path">The path of the resource being accessed.</param>
    /// <param name="data">Optional data object that can be used to store additional information relevant to the access evaluation process.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task EvaluateAsync(AccessOperation operation, string path, object? data = null, CancellationToken ct = default)
    {
        var context = BuildContext(operation, path, data);
        await EvaluateRulesAsync(context, operation, path, ct);
    }

    private async Task EvaluateRulesAsync(AccessContext<TResource> context, AccessOperation operation, string path, CancellationToken ct)
    {
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

            context = context with { Params = pathParams };

            if (await rule.EvaluateAsync(context, ct))
                return;

            throw new AccessDeniedException(operation, path);
        }

        throw new NoRulesMatchedException(operation, path);
    }

    private AccessContext<TResource> BuildContext(AccessOperation operation, string path, object? data)
    {
        return new AccessContext<TResource>
        {
            Operation = operation,
            Path = path,
            Data = data,
            Claims = contextAccessor.GetClaims(),
            GetResourceAsync = (ct) => resourceAccessor.GetAsync(path, ct),
        };
    }
}
