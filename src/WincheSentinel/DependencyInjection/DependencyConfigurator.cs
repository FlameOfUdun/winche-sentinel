using Microsoft.Extensions.DependencyInjection;
using WincheSentinel.Interfaces;

namespace WincheSentinel.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring dependencies related to resource access rules, caller context accessors, and resource object accessors for a specific resource type.
/// </summary>
/// <typeparam name="TResource">The type of the resource this configurator applies to.</typeparam>
/// <param name="services">The service collection to configure.</param>
public sealed class DependencyConfigurator<TResource>(IServiceCollection services) where TResource : class
{
    /// <summary>
    /// Adds an instance of a resource access rule to the service collection.
    /// </summary>
    /// <param name="instance">The instance of the resource access rule to add.</param>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddResourceAccessRule(IResourceAccessRule<TResource> instance)
    {
        ValidatePattern(instance.Path);
        services.AddSingleton(instance);
        return this;
    }

    /// <summary>
    /// Adds a resource access rule of the specified type to the service collection. The type must implement <see cref="IResourceAccessRule{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TRule">The type of the resource access rule to add.</typeparam>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddResourceAccessRule<TRule>() where TRule : class, IResourceAccessRule<TResource>
    {
        services.AddSingleton<IResourceAccessRule<TResource>, TRule>();
        return this;
    }

    /// <summary>
    /// Adds a caller context accessor of the specified type to the service collection. The type must implement <see cref="ICallerContextAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the caller context accessor to add.</typeparam>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddCallerContextAccessor<TAccessor>() where TAccessor : class, ICallerContextAccessor<TResource>
    {
        services.AddSingleton<ICallerContextAccessor<TResource>, TAccessor>();
        return this;
    }

    /// <summary>
    /// Adds an instance of a caller context accessor to the service collection. The instance must implement <see cref="ICallerContextAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the caller context accessor to add.</typeparam>
    /// <param name="instance">The instance of the caller context accessor to add.</param>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddCallerContextAccessor<TAccessor>(TAccessor instance) where TAccessor : class, ICallerContextAccessor<TResource>
    {
        services.AddSingleton<ICallerContextAccessor<TResource>>(instance);
        return this;
    }
    
    /// <summary>
    /// Adds a resource object accessor of the specified type to the service collection. The type must implement <see cref="IResourceObjectAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the resource object accessor to add.</typeparam>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddResourceObjectAccessor<TAccessor>() where TAccessor : class, IResourceObjectAccessor<TResource>
    {
        services.AddSingleton<IResourceObjectAccessor<TResource>, TAccessor>();
        return this;
    }

    /// <summary>
    /// Adds an instance of a resource object accessor to the service collection. The instance must implement <see cref="IResourceObjectAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the resource object accessor to add.</typeparam>
    /// <param name="instance">The instance of the resource object accessor to add.</param>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> AddResourceObjectAccessor<TAccessor>(TAccessor instance) where TAccessor : class, IResourceObjectAccessor<TResource>
    {
        services.AddSingleton<IResourceObjectAccessor<TResource>>(instance);
        return this;
    }

    private static void ValidatePattern(string? path)
    {
        if (path is null) return;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < segments.Length; i++)
        {
            var seg = segments[i];
            if (seg == "**" && i != segments.Length - 1)
                throw new ArgumentException($"'**' must be the last segment in path pattern '{path}'.");
            if (seg == "{}")
                throw new ArgumentException($"Empty parameter name '{{}}' is not valid in path pattern '{path}'.");
        }
    }
}
