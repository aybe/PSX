using Microsoft.Toolkit.Mvvm.DependencyInjection;
using PSX.Frontend.Core.ViewModels;

namespace PSX.Frontend.Windows;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
    }
}