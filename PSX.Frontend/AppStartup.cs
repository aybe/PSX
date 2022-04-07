using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    private static AppStartup? Instance { get; set; }

    public IHost Host { get; }

    public void SaveSettings()
    {
        // retrieve and serialize the actual settings

        var settings     = Host.Services.GetRequiredService<IOptions<AppSettings>>();
        var settingsJson = JsonConvert.SerializeObject(settings.Value);

        // get the path where settings should be written to

        var configuration = Host.Services.GetRequiredService<IConfiguration>();
        if (configuration is not IConfigurationRoot configurationRoot)
            throw new InvalidOperationException();

        var provider = configurationRoot.Providers.OfType<JsonConfigurationProvider>().Single();
        var fileInfo = provider.Source.FileProvider.GetFileInfo(provider.Source.Path);

        // only update relevant node in file as it may contain stuff unknown to us

        var text = File.ReadAllText(fileInfo.PhysicalPath);
        var tree = JsonConvert.DeserializeObject<JObject>(text) ?? throw new InvalidOperationException();
        var node = JObject.Parse(settingsJson);

        tree[nameof(AppSettings)] = node;

        var json = JsonConvert.SerializeObject(tree, Formatting.Indented);

        File.WriteAllText(fileInfo.PhysicalPath, json);
    }
}