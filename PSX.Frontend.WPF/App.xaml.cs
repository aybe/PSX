using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ProjectPSX.WPF.Frontend;
using ProjectPSX.WPF.Frontend.Services;

namespace ProjectPSX.WPF;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    public IServiceProvider Services { get; } = ConfigureServices();

    public new static App Current => (App)Application.Current;

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFilePickerService, FilePickerServiceWindows>();

        services.AddTransient<MainModel>();

        services.AddTransient<VideoOutputModel>();

        services.AddTransient<VideoMemoryModel>();

        var provider = services.BuildServiceProvider();

        return provider;
    }
}