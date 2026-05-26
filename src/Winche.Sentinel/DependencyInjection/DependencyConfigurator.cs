using Microsoft.Extensions.DependencyInjection;
using Winche.Sentinel.Interfaces;

namespace Winche.Sentinel.DependencyInjection;

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
    /// Adds a caller claims accessor of the specified type to the service collection. The type must implement <see cref="ICallerClaimsAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the caller claims accessor to add.</typeparam>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> SetCallerClaimsAccessor<TAccessor>() where TAccessor : class, ICallerClaimsAccessor<TResource>
    {
        services.AddSingleton<ICallerClaimsAccessor<TResource>, TAccessor>();
        return this;
    }

    /// <summary>
    /// Adds a caller claims accessor of the specified type to the service collection. The type must implement <see cref="ICallerClaimsAccessor{TResource}"/> and will be registered as a singleton.
    /// </summary>
    /// <typeparam name="TAccessor">The type of the caller claims accessor to add.</typeparam>
    /// <param name="instance">The instance of the caller claims accessor to add.</param>
    /// <returns>The current <see cref="DependencyConfigurator{TResource}"/> instance.</returns>
    public DependencyConfigurator<TResource> SetCallerClaimsAccessor<TAccessor>(TAccessor instance) where TAccessor : class, ICallerClaimsAccessor<TResource>
    {
        services.AddSingleton<ICallerClaimsAccessor<TResource>>(instance);
        return this;
    }
}
