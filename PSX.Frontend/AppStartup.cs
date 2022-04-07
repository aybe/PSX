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

        var host =
            new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("AppSettings.json", false)
                        ;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)))
                        .AddSingleton<INavigationService, NavigationService>()
                        .AddSingleton<IEmulatorControlService, EmulatorControlService>()
                        .AddSingleton<IEmulatorDisplayService, EmulatorDisplayService>()
                        .AddSingleton<MainModel>()
                        .AddSingleton<MainViewModel>()
                        .AddSingleton<MainViewModelCommands>()
                        .AddTransient<VideoViewModel>()
                        ;
                });

        action?.Invoke(host);

        Host = host.Build();
    }

    public static AppStartup? Instance { get; private set; }

    public IHost Host { get; }
}