using PSX.Frontend.Core.ViewModels;

namespace PSX.Frontend.Windows;

public partial class MainWindow
{
    public MainWindow(MainViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}