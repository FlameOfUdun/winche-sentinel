# Winche.Sentinel

Path-based authorization middleware for .NET. Define access rules per resource type using path patterns, and evaluate them against caller claims and operations.

## Install

```cmd
dotnet add package Winche.Sentinel
```

## Quick Start

```csharp
services.AddWincheSentinel<Document>(c =>
{
    c.AddResourceObjectAccessor<DocumentAccessor>();
    c.AddCallerContextAccessor<HttpContextClaimsAccessor>();
    c.AddResourceAccessRule(new DocumentRule("/docs/{id}", [AccessOperation.Read, AccessOperation.Write]));
});
```

Inject `IAccessRuleEvaluator<Document>` and call it before performing the operation:

```csharp
// throws AccessDeniedException or NoRulesMatchedException on failure
await evaluator.EvaluateAsync(AccessOperation.Read, "/docs/42");
```

## How It Works

1. The evaluator matches the incoming path against each registered rule's path pattern.
2. The first matching rule's `EvaluateAsync` is called with an `AccessContext<TResource>`.
3. Return `true` to grant access, `false` to deny (`AccessDeniedException` is thrown).
4. If no rule matches, `NoRulesMatchedException` is thrown.

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
using WincheSentinel.Interfaces;
using WincheSentinel.Models;

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

## AccessContext

| Member | Description |
| ------ | ----------- |
| `Operation` | `Read`, `Write`, or `Delete` |
| `Path` | The path being accessed |
| `Params` | Path parameters extracted from the pattern |
| `Claims` | Caller claims from `ICallerContextAccessor` |
| `GetResourceAsync` | Lazy loader for the resource object |
| `Data` | Optional extra data passed to `EvaluateAsync` |

## Multiple Resource Types

Call `AddWincheSentinel<T>()` once per resource type — each gets its own isolated rule set and evaluator.

## License

Elastic License 2.0
