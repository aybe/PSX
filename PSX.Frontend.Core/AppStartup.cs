using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core.Modules;
using PSX.Frontend.Core.Navigation;

namespace PSX.Frontend.Core;

public sealed class AppStartup
{
    private static AppStartup? _current;

    public AppStartup(Action<IHostBuilder>? action = null)
    {
        _current = _current is null ? this : throw new InvalidOperationException("Only a single instance can be registered.");

        var builder =
            new HostBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices);

        action?.Invoke(builder);

        Host = builder.Build();
    }

    public static AppStartup Current => _current ?? throw new InvalidOperationException();

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
            .AddScoped<INavigationService, NavigationService>()
            .AddSingleton<ViewModelShell>()
            .AddSingleton<ViewModelLog>()
            ;
    }
}