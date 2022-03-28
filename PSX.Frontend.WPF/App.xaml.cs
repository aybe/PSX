using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core;
using PSX.Frontend.Core.Modules;
using PSX.Frontend.Core.Services;
using PSX.Frontend.WPF.Frontend;
using PSX.Frontend.WPF.Frontend.Shared;
using PSX.Frontend.WPF.Services;
using PSX.Frontend.WPF.Views;
using PSX.Logging;

namespace PSX.Frontend.WPF;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    private AppStartup AppStartup { get; set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppStartup = new AppStartup(ConfigureHost);

        void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(e.Args);
                })
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddTransient<MainModel>() // TODO move content of it to core module
                        .AddSingleton<IOpenFileService, OpenFileService>()
                        .AddSingleton<IApplicationService, ApplicationService>()
                        .AddSingleton<IViewShell, ViewShell>()
                        .AddSingleton<IViewLog, ViewLog>()
                        ;
                })
                .ConfigureLogging((_, builder) =>
                {
                    builder.AddObservable();
                });
        }

        await AppStartup.Host.StartAsync();

        var shell = AppStartup.Host.Services.GetRequiredService<IViewShell>();

        shell.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        using (AppStartup.Host)
        {
            AppStartup.Host.StopAsync(TimeSpan.FromSeconds(5.0d));
        }

        // BUG it will crash here because of console -> System.IO.IOException: 'The parameter is incorrect.' -> https://github.com/dotnet/runtime/issues/62192
    }
}