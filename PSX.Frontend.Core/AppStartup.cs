using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core.Models;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.Core.ViewModels;

namespace PSX.Frontend.Core;

public sealed class AppStartup
{
    public AppStartup(Action<IHostBuilder>? action = null)
    {
        Instance = Instance is null
            ? this
            : throw new InvalidOperationException("Only a single instance is permitted");

        var hostBuilder =
            new HostBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices);

        action?.Invoke(hostBuilder);

        Host = hostBuilder.Build();
    }

    private static AppStartup? Instance { get; set; }

    public IHost Host { get; }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("AppSettings.json", false);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services
            .Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)))
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<MainModel>()
            .AddSingleton<MainViewModel>();
    }
}