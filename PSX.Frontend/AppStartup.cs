using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Interface;
using PSX.Frontend.Services.Emulation;
using PSX.Frontend.Services.Navigation;

namespace PSX.Frontend;

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
            .AddSingleton<IEmulatorControlService, EmulatorControlService>()
            .AddSingleton<IEmulatorDisplayService, EmulatorDisplayService>()
            .AddSingleton<MainModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<VideoScreenModel>()
            .AddTransient<VideoViewModel>()
            ;
    }
}