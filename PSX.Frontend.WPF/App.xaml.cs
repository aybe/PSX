using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PSX.Frontend.Interface;
using PSX.Frontend.Services;
using PSX.Frontend.Services.Navigation;
using PSX.Frontend.WPF.Services;
using PSX.Frontend.WPF.Windows;

namespace PSX.Frontend.WPF;

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
                        .AddSingleton<ITextDialogService, TextDialogService>()
                        .AddSingleton<IFileService, FileService>()
                        .AddTransient<IMainView, MainWindow>()
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
            var settings = AppStartup.Host.Services.GetRequiredService<IOptions<AppSettings>>();

            settings.Value.Save();

            using (AppStartup.Host)
            {
                AppStartup.Host.StopAsync(TimeSpan.FromSeconds(5.0d));
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Application Error");
            throw;
        }
    }
}