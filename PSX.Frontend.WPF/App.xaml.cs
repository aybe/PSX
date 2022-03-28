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

    public new static App Current => (App)Application.Current;

    private IHost Host { get; set; } = null!;

    public IServiceProvider Services => Host.Services;

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
                        // older ones
                        .AddSingleton<IFilePickerService, FilePickerServiceWindows>()
                        .AddSingleton<IApplication, IApplicationWpf>()
                        .AddTransient<MainModel>() // TODO this should be the one in Core and updated with existing stuff
                        // newer ones
                        .AddSingleton<MainWindow>() // TODO do the same as for log view
                        .AddTransient<MainViewModel>()
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

        var window = Host.Services.GetRequiredService<MainWindow>(); // TODO update this

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