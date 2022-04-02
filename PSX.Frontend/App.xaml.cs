using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PSX.Frontend.Core;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.Views;
using PSX.Frontend.Services;
using PSX.Frontend.Windows;

namespace PSX.Frontend;

public partial class App : IApplicationService
{
    private AppStartup AppStartup { get; set; } = null!;

    bool IApplicationService.TryGetView<T>(out T? result) where T : class
    {
        result = Windows.OfType<T>().FirstOrDefault();
        return result is not null;
    }

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
                        .AddSingleton<IApplicationService>(this)
                        .AddSingleton<IStorageService, StorageService>()
                        .AddTransient<IMainView, MainWindow>()
                        .AddTransient<ILoggingView, LoggingWindow>()
                        .AddTransient<IVideoScreenView, VideoScreenWindow>()
                        .AddTransient<IVideoMemoryView, VideoMemoryWindow>()
                        ;
                });
        }

        await AppStartup.Host.StartAsync();

        // since we use DI at this point, we can't use StartupUri

        var navigationService = AppStartup.Host.Services.GetRequiredService<INavigationService>();

        navigationService.Navigate<IMainView>();
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