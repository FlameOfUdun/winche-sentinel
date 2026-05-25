using WincheSentinel.Interfaces;

namespace WincheSentinel.Services;

internal sealed class ResourceObjectAccessor<TResource> : IResourceObjectAccessor<TResource> where TResource : class
{
    public Task<TResource?> GetAsync(string path, CancellationToken ct = default) => Task.FromResult<TResource?>(null);
}
