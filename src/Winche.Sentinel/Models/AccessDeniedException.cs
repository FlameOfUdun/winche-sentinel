namespace Winche.Sentinel.Models;

/// <summary>
/// Represents an exception that is thrown when access to a resource is denied based on the evaluation of access rules. This exception includes information about the access operation being performed and the path of the resource for which access was denied, which can be used for logging, debugging, or providing feedback to the caller about why access was denied.
/// </summary>
/// <param name="operation">The access operation that was denied.</param>
/// <param name="path">The path of the resource for which access was denied.</param>
public sealed class AccessDeniedException(AccessOperation operation, string path)
    : Exception($"Access denied: {operation} on '{path ?? "resource"}'");

/// <summary>
/// Represents an exception that is thrown when no access rules match the access context during access evaluation, resulting in a default denial of access. This exception includes information about the access operation being performed and the path of the resource for which no rules matched, which can be used for logging, debugging, or providing feedback to the caller about why access was denied due to the absence of matching rules.
/// </summary>
/// <param name="operation">The access operation that was denied due to no matching rules.</param>
/// <param name="path">The path of the resource for which no rules matched.</param>
public sealed class NoRulesMatchedException(AccessOperation operation, string path)
    : Exception($"Access denied: {operation} on '{path ?? "resource"}'");
