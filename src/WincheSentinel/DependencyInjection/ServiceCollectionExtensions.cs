using Microsoft.Extensions.DependencyInjection;
using WincheSentinel.Interfaces;
using WincheSentinel.Services;

namespace WincheSentinel.DependencyInjection;

/// <summary>
/// Provides extension methods for registering WincheSentinel services and configuring resource access rules, caller context accessors, and resource object accessors in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core services required for WincheSentinel to function, including path pattern matching, access rule evaluation, caller context access, and resource object access. This method should be called before configuring specific resource access rules and accessors.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource for which to register services.</typeparam>
    /// <param name="services">The service collection to which the services will be added.</param>
    /// <param name="configure">An optional action to further configure resource access rules, caller context accessors, and resource object accessors using the <see cref="DependencyConfigurator{TResource}"/>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddWincheSentinel<TResource>(this IServiceCollection services, Action<DependencyConfigurator<TResource>>? configure = null) where TResource : class 
    {
        services.AddSingleton<IPathPatternMatcher<TResource>, PathPatternMatcher<TResource>>();
        services.AddSingleton<IAccessRuleEvaluator<TResource>, AccessRuleEvaluator<TResource>>();
        services.AddSingleton<ICallerContextAccessor<TResource>, CallerContextAccessor<TResource>>();
        services.AddSingleton<IResourceObjectAccessor<TResource>, ResourceObjectAccessor<TResource>>();

        configure?.Invoke(new DependencyConfigurator<TResource>(services));

        return services;
    }
}
