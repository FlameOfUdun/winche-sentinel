# Winche.Sentinel

Path-based authorization library for .NET. Define access rules per resource type using path patterns, and evaluate them against caller claims and operations.

## Install

```cmd
dotnet add package Winche.Sentinel
```

## Quick Start

```csharp
services.AddWincheSentinel<Document>(c =>
{
    c.SetCallerClaimsAccessor<HttpContextClaimsAccessor>();
    c.AddResourceAccessRule(new DocumentRule("/docs/{id}", [AccessOperation.Read, AccessOperation.Write]));
});
```

Inject `IAccessRuleEvaluator<Document>` and call it before performing the operation:

```csharp
// throws AccessDeniedException or NoRulesMatchedException on failure
await evaluator.EvaluateAsync(
    AccessOperation.Read,
    "/docs/42",
    loader: ct => repository.GetByIdAsync(42, ct));
```

## How It Works

For a given operation and path, the evaluator considers **every** registered rule that matches the request:

1. A rule matches when its path pattern matches the path **and** its operations set contains the operation. (A `null` path matches every path; a `null` operations set matches every operation.)
2. Each matching rule's `EvaluateAsync` is called with an `AccessContext<TResource>`. Returning `true` **grants** access — evaluation stops and the request is allowed.
3. A matching rule returning `false` does **not** deny; it simply doesn't grant, and evaluation continues to the next matching rule.
4. Access is the default-deny outcome: if at least one rule matched but none granted, `AccessDeniedException` is thrown; if no rule matched the path and operation at all, `NoRulesMatchedException` is thrown.

These are **OR / grant-only** semantics (Firestore-style): there is no explicit deny, and registration order does **not** affect the decision. Because a grant cannot be revoked by another rule, **grant narrowly** — don't write a broad `**` grant and expect a more specific rule to restrict it; grant access only where it should be allowed and let default-deny cover the rest.

## Path Patterns

| Pattern | Matches |
| ------- | ------- |
| `/docs/42` | Exact match |
| `/docs/{id}` | Any single segment; captured as `Params["id"]` |
| `/docs/*` | Any single segment (unnamed) |
| `/docs/**` | Zero or more remaining segments; captured as `Params["**"]` |

`**` must be the last segment in the pattern.

## Implementing a Rule

```csharp
using Winche.Sentinel.Interfaces;
using Winche.Sentinel.Models;

public class DocumentRule(string path, IEnumerable<AccessOperation> ops) : IResourceAccessRule<Document>
{
    public string Path => path;
    public IReadOnlySet<AccessOperation> Operations => new HashSet<AccessOperation>(ops);

    public async Task<bool> EvaluateAsync(AccessContext<Document> context, CancellationToken ct)
    {
        var doc = await context.GetResourceAsync(ct);
        var userId = context.Claims["sub"] as string;
        return doc?.OwnerId == userId;
    }
}
```

Register via instance or by type:

```csharp
// instance
c.AddResourceAccessRule(new DocumentRule("/docs/{id}", [AccessOperation.Read]));

// type (resolved from DI)
c.AddResourceAccessRule<DocumentRule>();
```

## AccessContext

| Member | Description |
| ------ | ----------- |
| `Operation` | `Read`, `Write`, `Delete`, or `Aggregate` |
| `Path` | The path being accessed |
| `Params` | Path parameters extracted from the pattern |
| `Claims` | Caller claims from `ICallerClaimsAccessor` |
| `GetResourceAsync` | Lazy loader — invokes the `loader` function passed to `EvaluateAsync` |
| `IncomingData` | Optional extra data passed to `EvaluateAsync` |
| `GetIncomingData<T>()` | Casts `IncomingData` to `T`; returns `null` if the cast fails |

## Caller Claims

Implement `ICallerClaimsAccessor<TResource>` to supply caller claims (e.g. from `HttpContext`):

```csharp
public class HttpContextClaimsAccessor : ICallerClaimsAccessor<Document>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextClaimsAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public IReadOnlyDictionary<string, object?> GetClaims()
        => _httpContextAccessor.HttpContext?.User.Claims
            .ToDictionary(c => c.Type, c => (object?)c.Value)
           ?? new Dictionary<string, object?>();
}
```

Register it via `SetCallerClaimsAccessor`:

```csharp
c.SetCallerClaimsAccessor<HttpContextClaimsAccessor>();
// or pass an instance directly:
c.SetCallerClaimsAccessor(new HttpContextClaimsAccessor(httpContextAccessor));
```

If no claims accessor is registered, `EmptyCallerClaimsAccessor` is used (returns an empty dictionary).

## Passing Incoming Data

Pass a write payload through to the rule via the `data` parameter:

```csharp
await evaluator.EvaluateAsync(AccessOperation.Write, "/docs/42", data: updateRequest);
```

In the rule, retrieve it with `GetIncomingData<T>()`:

```csharp
public async Task<bool> EvaluateAsync(AccessContext<Document> context, CancellationToken ct)
{
    var update = context.GetIncomingData<UpdateDocumentRequest>();
    // ...
}
```

## Post-Registration Configuration

Use `ConfigureWincheSentinel<T>()` to add rules or swap the claims accessor after the initial `AddWincheSentinel<T>()` call:

```csharp
services.AddWincheSentinel<Document>();

// elsewhere (e.g. a feature module)
services.ConfigureWincheSentinel<Document>(c =>
{
    c.AddResourceAccessRule(new ArchiveRule("/docs/{id}/archive", [AccessOperation.Write]));
});
```

## Multiple Resource Types

Call `AddWincheSentinel<T>()` once per resource type — each gets its own isolated rule set and evaluator.

```csharp
services.AddWincheSentinel<Document>(c => { /* ... */ });
services.AddWincheSentinel<Project>(c => { /* ... */ });
```

## License

Elastic License 2.0
