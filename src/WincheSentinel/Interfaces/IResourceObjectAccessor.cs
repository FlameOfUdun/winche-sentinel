namespace WincheSentinel.Interfaces;

/// <summary>
/// Defines an accessor for retrieving resource objects based on a given path. This interface abstracts the logic for fetching resource instances, allowing for flexible implementations that can retrieve resources from various sources such as databases, APIs, or in-memory collections.
/// </summary>
/// <typeparam name="TResource">The type of the resource this accessor retrieves.</typeparam>
public interface IResourceObjectAccessor<TResource> where TResource : class
{
    /// <summary>
    /// Asynchronously retrieves a resource object based on the provided path. The implementation of this method should contain the logic to fetch the resource instance corresponding to the given path, which may involve querying a database, calling an external API, or looking up an in-memory collection. The method returns a task that represents the asynchronous operation, and the result contains the retrieved resource object or null if no matching resource is found.
    /// </summary>
    /// <param name="path">The path identifying the resource to retrieve.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the retrieved resource object or null if no matching resource is found.</returns>
    Task<TResource?> GetAsync(string path, CancellationToken ct = default);
}
