using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using PSX.Frontend.Core.Models;
using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.Services;
using PSX.Frontend.Windows;

namespace PSX.Frontend;

public partial class App
{
    public App()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<IFileDialogService, FileDialogService>()
                .AddSingleton<IShutdownService, ShutdownService>()
                .AddTransient<MainModel>()
                .AddTransient<MainViewModel>()
                .AddTransient<MainWindow>()
                .BuildServiceProvider()
        );
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = Ioc.Default.GetRequiredService<MainWindow>();

        window.Show();
    }
}