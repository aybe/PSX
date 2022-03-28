using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core;
using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.WPF.Frontend;
using PSX.Frontend.WPF.Frontend.Shared;
using PSX.Frontend.WPF.Frontend.Views;
using PSX.Logging;

namespace PSX.Frontend.WPF;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    [Obsolete]
    public IServiceProvider Services { get; } = ConfigureServices();

    public new static App Current => (App)Application.Current;

    private IHost Host { get; set; } = null!;

    public IServiceProvider ServiceProvider => Host.Services;

    [Obsolete]
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFilePickerService, FilePickerServiceWindows>();

        services.AddSingleton<IApplication, IApplicationWpf>();

        services.AddTransient<MainModel>();

        var provider = services.BuildServiceProvider();

        return provider;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("AppSettings.json", false)
                        .AddCommandLine(e.Args);
                })
                .ConfigureServices((context, collection) =>
                {
                    collection
                        .Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)))
                        .AddSingleton<MainWindow>()
                        .AddTransient<MainViewModel>() // TODO this should be the one in Core and updated with existing stuff
                        .AddSingleton<ILogView, LogView>()
                        .AddSingleton<ILogViewModel, LogViewModel>()
                        ;
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddObservable();
                })
            ;

        Host = hostBuilder.Build();

        await Host.StartAsync();

        var window = Host.Services.GetRequiredService<MainWindow>();

        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        using (Host)
        {
            Host.StopAsync(TimeSpan.FromSeconds(5.0d));
            // BUG System.IO.IOException: 'The parameter is incorrect.' -> remove console https://github.com/dotnet/runtime/issues/62192
        }
    }
}