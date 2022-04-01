using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.Core.Views;

namespace PSX.Frontend.Windows;

public partial class MainWindow : IMainView
{
    public MainWindow(MainViewModel model)
    {
        InitializeComponent();

        DataContext = model;
    }
}