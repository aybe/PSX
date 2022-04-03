using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core;
using PSX.Frontend.Core.Old;
using PSX.Frontend.Core.Old.Models;
using PSX.Frontend.Core.Old.Navigation;
using PSX.Frontend.Core.Old.Services;
using PSX.Frontend.Core.Services.Emulator;
using PSX.Frontend.WPF.Services;
using PSX.Frontend.WPF.Views;
using PSX.Logging;
using AppStartup = PSX.Frontend.Core.Old.AppStartup;

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
                        .AddSingleton<IOpenFileService, OpenFileService>()
                        .AddSingleton<IApplicationService, ApplicationService>()
                        .AddSingleton<EmulatorService>()
                        .AddSingleton<IShellView, ShellView>()
                        .AddTransient<ILogView, LogView>()
                        ;
                })
                .ConfigureLogging((_, builder) =>
                {
                    builder.AddObservable();
                });
        }

        await AppStartup.Host.StartAsync();

        var navigationService = AppStartup.Host.Services.GetRequiredService<INavigationService>();

        navigationService.Navigate<IShellView>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        try
        {
            using (AppStartup.Host)
            {
                AppStartup.Host.StopAsync(TimeSpan.FromSeconds(5.0d));
            }
        }
        catch (IOException)
        {
            // BUG it will crash here because of console -> System.IO.IOException: 'The parameter is incorrect.' -> https://github.com/dotnet/runtime/issues/62192
        }
    }
}