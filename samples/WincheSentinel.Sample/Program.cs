using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WincheSentinel.DependencyInjection;
using WincheSentinel.Interfaces;
using WincheSentinel.Models;
using WincheSentinel.Sample.Models;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddWincheSentinel<ResourceOne>(c =>
        {
            c.AddResourceObjectAccessor(new ResourceAccessorOne());
            c.AddResourceAccessRule(new AccessRuleOne("/allowed", [AccessOperation.Read], async (context, ct) =>
            {
                var resource = await context.GetResourceAsync(ct);
                Console.WriteLine($"Allowing access to resource with value: {resource?.Value}");
                return await Task.FromResult(true);
            }));
            c.AddResourceAccessRule(new AccessRuleOne("/denied", [AccessOperation.Read], async (context, ct) =>
            {
                var resource = await context.GetResourceAsync(ct);
                Console.WriteLine($"Denying access to resource with value: {resource?.Value}");
                return await Task.FromResult(false);
            }));
        });

        services.AddWincheSentinel<ResourceTwo>(c =>
        {
            c.AddResourceObjectAccessor(new ResourceAccessorTwo());
            c.AddResourceAccessRule(new AccessRuleTwo("/allowed", [AccessOperation.Read], async (context, ct) =>
            {
                var resource = await context.GetResourceAsync(ct);
                Console.WriteLine($"Allowing access to resource with value: {resource?.Value}");
                return await Task.FromResult(true);
            }));
            c.AddResourceAccessRule(new AccessRuleTwo("/denied", [AccessOperation.Read], async (context, ct) =>
            {
                var resource = await context.GetResourceAsync(ct);
                Console.WriteLine($"Denying access to resource with value: {resource?.Value}");
                return await Task.FromResult(false);
            }));
        });
    })
    .Build();

using var scope = host.Services.CreateScope();

try
{
    await scope.ServiceProvider.GetRequiredService<IAccessRuleEvaluator<ResourceOne>>().EvaluateAsync(AccessOperation.Read, "/allowed");
    Console.WriteLine("Allowed");
}
catch (AccessDeniedException)
{
    Console.WriteLine("Denied");
}
catch (NoRulesMatchedException)
{
    Console.WriteLine("No Match");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected exception: {ex}");
}

try
{
    await scope.ServiceProvider.GetRequiredService<IAccessRuleEvaluator<ResourceOne>>().EvaluateAsync(AccessOperation.Read, "/denied");
}
catch (AccessDeniedException)
{
    Console.WriteLine("Denied");
}
catch (NoRulesMatchedException)
{
    Console.WriteLine("No Match");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected exception: {ex}");
}

try
{
    await scope.ServiceProvider.GetRequiredService<IAccessRuleEvaluator<ResourceTwo>>().EvaluateAsync(AccessOperation.Read, "/allowed");
    Console.WriteLine("Allowed");
}
catch (AccessDeniedException)
{
    Console.WriteLine("Denied");
}
catch (NoRulesMatchedException)
{
    Console.WriteLine("No Match");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected exception: {ex}");
}

try
{
    await scope.ServiceProvider.GetRequiredService<IAccessRuleEvaluator<ResourceTwo>>().EvaluateAsync(AccessOperation.Read, "/denied");
}
catch (AccessDeniedException)
{
    Console.WriteLine("Denied");
}
catch (NoRulesMatchedException)
{
    Console.WriteLine("No Match");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected exception: {ex}");
}

host.Run();