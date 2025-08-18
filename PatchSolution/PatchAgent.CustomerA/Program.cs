using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using PatchAgent.CustomerA;
using PatchAgent.CustomerA.Services;
using PatchAgent.CustomerA.Utilities;
using SharedLibraryAgents.Interfaces;
using SharedLibraryAgents.Models;

Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        // Configure settings
        services.Configure<AgentSettings>(hostContext.Configuration.GetSection("AgentSettings"));
        services.Configure<PatchPathSettings>(hostContext.Configuration.GetSection("PatchPaths"));

        // Register HttpClient for ApiUtility
        services.AddHttpClient<ApiUtility>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["AgentSettings:PatchServerUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("PatchServerUrl is not configured in appsettings.json");

            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            client.BaseAddress = new Uri(baseUrl);
        });

        // Register all utility services
        services.AddSingleton<VersionUtility>();
        services.AddSingleton<FileUtility>();
        services.AddSingleton<LoggingUtility>();
        services.AddSingleton<CleanupUtility>();
        // ApiUtility is already registered above with AddHttpClient

        // Register the main service
        services.AddSingleton<IPatchAgentService, PatchAgentService>();

        // Register the hosted service
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();