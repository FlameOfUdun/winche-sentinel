using System.Collections.Immutable;

namespace Winche.Sentinel.Models;

/// <summary>
/// Defines the type of access operation being performed on a resource.
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

    /// <summary>
    /// Represents an aggregation operation over a collection, such as running an aggregation
    /// pipeline (count, sum, group, lookup, …). Distinct from <see cref="Read"/>: granting read
    /// access to a collection's documents does not authorize aggregating across them, because an
    /// aggregate result can reveal information about documents the caller cannot read individually.
    /// </summary>
    Aggregate,
}

/// <summary>
/// Represents the context of an access request for a specific resource
/// </summary>
/// <typeparam name="TResource">The type of the resource for which access is being evaluated.</typeparam>
public sealed record AccessContext<TResource> where TResource : class
{
    /// <summary>
    /// The type of access operation being performed.
    /// </summary>
    public required AccessOperation Operation { get; init; }

    /// <summary>
    /// Path of the resource being accessed.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Claims associated with the access request.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Claims { get; init; } = ImmutableDictionary<string, object?>.Empty;

    /// <summary>
    /// Parameters associated with the access request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Params { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Resource loader function.
    /// </summary>
    public required Func<CancellationToken, Task<TResource?>> GetResourceAsync { get; init; }

    /// <summary>
    /// Optional incoming write data.
    /// </summary>
    public object? IncomingData { get; init; } = null;

    /// <summary>
    /// Attempts to retrieve the incoming data as the specified type. Returns null if the data is not of the expected type or if there is no incoming data.
    /// </summary>
    /// <typeparam name="TData">The type to which the incoming data should be cast.</typeparam>
    /// <returns>The incoming data cast to the specified type, or null if the cast is not possible.</returns>
    public TData? GetIncomingData<TData>()
    {
        if (IncomingData is TData data)
            return data;

        return default;
    } 
}
