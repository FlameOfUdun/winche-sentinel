using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;

namespace Winche.Sentinel.Sample.Models;

public class AccessRuleOne(string path, IEnumerable<AccessOperation> operations, Func<AccessContext<ResourceOne>, CancellationToken, Task<bool>> evaluate) : IResourceAccessRule<ResourceOne>
{
    public string Path => path;
    public IReadOnlySet<AccessOperation> Operations => new HashSet<AccessOperation>(operations);
    public Task<bool> EvaluateAsync(AccessContext<ResourceOne> context, CancellationToken ct) => evaluate(context, ct);
}

public class AccessRuleTwo(string path, IEnumerable<AccessOperation> operations, Func<AccessContext<ResourceTwo>, CancellationToken, Task<bool>> evaluate) : IResourceAccessRule<ResourceTwo>
{
    public string Path => path;
    public IReadOnlySet<AccessOperation> Operations => new HashSet<AccessOperation>(operations);
    public Task<bool> EvaluateAsync(AccessContext<ResourceTwo> context, CancellationToken ct) => evaluate(context, ct);
}