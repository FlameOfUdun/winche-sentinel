using WincheSentinel.Interfaces;

namespace WincheSentinel.Sample.Models;

public class ResourceAccessorOne : IResourceObjectAccessor<ResourceOne>
{
    public Task<ResourceOne?> GetAsync(string path, CancellationToken ct = default) => Task.FromResult<ResourceOne?>(new ResourceOne(1));
}

public class ResourceAccessorTwo : IResourceObjectAccessor<ResourceTwo>
{
    public Task<ResourceTwo?> GetAsync(string path, CancellationToken ct = default) => Task.FromResult<ResourceTwo?>(new ResourceTwo(2));
}
