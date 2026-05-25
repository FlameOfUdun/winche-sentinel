using System.Collections.Immutable;

namespace WincheSentinel.Models;

/// <summary>
/// Defines the type of access operation being performed on a resource, such as reading, writing, or deleting. This enumeration is used in the <see cref="AccessContext{TResource}"/> to specify the intended operation for which access is being evaluated.
/// </summary>
public enum AccessOperation 
{
    /// <summary>
    /// Represents a read operation on a resource, such as retrieving data or viewing information.
    /// </summary>
    Read,

    /// <summary>
    /// Represents a write operation on a resource, such as creating, updating, or modifying data.
    /// </summary>
    Write,

    /// <summary>
    /// Represents a delete operation on a resource, such as removing data or deleting an entity.
    /// </summary>
    Delete,
}

/// <summary>
/// Represents the context of an access request for a specific resource, including the operation being performed, the path of the resource, any claims or parameters associated with the request, and a function to retrieve the resource object if needed. This context is used during access rule evaluation to determine whether access should be granted or denied based on the defined rules and the current state of the resource and caller context.
/// </summary>
/// <typeparam name="TResource">The type of the resource for which access is being evaluated.</typeparam>
public sealed record AccessContext<TResource> where TResource : class
{
    /// <summary>
    /// Gets the type of access operation being performed on the resource, such as reading, writing, or deleting. This property is required and must be set when creating an instance of <see cref="AccessContext{TResource}"/>. The value of this property is used during access rule evaluation to determine which rules apply to the current operation and how access should be evaluated based on the defined rules for that operation type.
    /// </summary>
    public required AccessOperation Operation { get; init; }

    /// <summary>
    /// Gets the path of the resource being accessed, which is used for matching against path patterns defined in access rules. This property is required and must be set when creating an instance of <see cref="AccessContext{TResource}"/>. The value of this property is used during access rule evaluation to determine which rules apply to the current resource based on the defined path patterns in the rules.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets a read-only dictionary of claims associated with the access request, which can be used for evaluating access rules that depend on specific claims or attributes of the caller. This property is optional and can be left empty if there are no relevant claims for the access evaluation. The keys in the dictionary represent the claim types, and the values represent the claim values. Access rules can use this information to make decisions based on the caller's identity, roles, permissions, or other attributes represented by the claims.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Claims { get; init; } = ImmutableDictionary<string, object?>.Empty;

    /// <summary>
    /// Gets a read-only dictionary of parameters associated with the access request, which can be used for evaluating access rules that depend on specific parameters or contextual information about the request. This property is optional and can be left empty if there are no relevant parameters for the access evaluation. The keys in the dictionary represent the parameter names, and the values represent the parameter values. Access rules can use this information to make decisions based on additional context about the request, such as query parameters, headers, or other relevant data that may influence access decisions.
    /// </summary>
    public IReadOnlyDictionary<string, string> Params { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets a function that retrieves the resource object associated with the access request, if needed for access evaluation. This property is required and must be set when creating an instance of <see cref="AccessContext{TResource}"/>. The function takes a <see cref="CancellationToken"/> as a parameter and returns a task that resolves to the resource object of type <typeparamref name="TResource"/> or null if the resource cannot be retrieved. Access rules can use this function to obtain the current state of the resource during access evaluation, which may be necessary for making informed decisions about whether to grant or deny access based on the resource's properties, state, or other relevant information.
    /// </summary>
    public required Func<CancellationToken, Task<TResource?>> GetResourceAsync { get; init; }

    /// <summary>
    /// Gets an optional data object that can be used to store additional information relevant to the access evaluation process. This property is optional and can be left null if there is no additional data to store. The value of this property can be set by the caller or by access rules during the evaluation process to provide additional context or information that may be useful for making access decisions. Access rules can read from or write to this property as needed during the evaluation process.
    /// </summary>
    public object? Data { get; init; } = null;
}
