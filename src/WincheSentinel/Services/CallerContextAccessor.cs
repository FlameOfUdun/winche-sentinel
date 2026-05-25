using WincheSentinel.Interfaces;

namespace WincheSentinel.Services;

internal sealed class CallerContextAccessor<TResource> : ICallerContextAccessor<TResource> where TResource : class
{
    private readonly IReadOnlyDictionary<string, object?> _claims = new Dictionary<string, object?>();

    public IReadOnlyDictionary<string, object?> GetClaims() => _claims;
}