using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using PSX.Frontend.Core.Services;
using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.Services;

namespace PSX.Frontend;

public partial class App
{
    public App()
    {
        InitializeComponent();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<IFileDialogService, FileDialogService>()
                .AddSingleton<IShutdownService, ShutdownService>()
                .AddTransient<MainViewModel>()
                .BuildServiceProvider()
        );
    }
}