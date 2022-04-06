using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Interface;
using PSX.Frontend.Services.Emulation;
using PSX.Frontend.Services.Navigation;
using PSX.Frontend.Services.Options;

namespace PSX.Frontend;

public sealed class AppStartup
{
    public AppStartup(Action<IHostBuilder>? action = null)
    {
        Instance = Instance is null
            ? this
            : throw new InvalidOperationException("Only a single instance is permitted");

        var root = (IConfigurationRoot)null!;

        var host =
            new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    root = builder
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("AppSettings.json", false)
                        .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddSingleton(root as IConfiguration)
                        .AddSingleton(root)
                        .ConfigureWritable<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)))
                        .AddSingleton<INavigationService, NavigationService>()
                        .AddSingleton<IEmulatorControlService, EmulatorControlService>()
                        .AddSingleton<IEmulatorDisplayService, EmulatorDisplayService>()
                        .AddSingleton<MainModel>()
                        .AddSingleton<MainViewModel>()
                        .AddTransient<VideoViewModel>();
                });

        action?.Invoke(host);

        Host = host.Build();
    }

    private static AppStartup? Instance { get; set; }

    public IHost Host { get; }
}