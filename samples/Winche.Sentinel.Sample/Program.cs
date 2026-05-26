using Microsoft.Extensions.Hosting;
using Winche.Sentinel.DependencyInjection;
using Winche.Sentinel.Models;
using Winche.Sentinel.Sample.Models;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddWincheSentinel<ResourceOne>(c =>
        {
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

host.Run();