using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.Core.Services;
using PSX.Frontend.Services;
using PSX.Frontend.Windows;

namespace PSX.Frontend;

public partial class App
{
    private AppStartup AppStartup { get; set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppStartup = new AppStartup(ConfigureHost);

        void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(e.Args);
                })
                .ConfigureServices((_, collection) =>
                {
                    collection
                        .AddSingleton<IFileDialogService, FileDialogService>()
                        .AddSingleton<IShutdownService, ShutdownService>()
                        .AddTransient<MainWindow>();
                });
        }

        await AppStartup.Host.StartAsync();

        // since we use DI at this point, we can't use StartupUri

        var navigationService = AppStartup.Host.Services.GetRequiredService<INavigationService>();

        navigationService.Navigate<MainWindow>();
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
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
}