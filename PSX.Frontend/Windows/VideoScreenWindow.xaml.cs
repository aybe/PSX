using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.Core.Views;

namespace PSX.Frontend.Windows;

public partial class VideoScreenWindow : IVideoScreenView
{
    public VideoScreenWindow(ViewScreenViewModel model)
    {
        InitializeComponent();

        DataContext = model;

        Loaded += (_, _) =>
        {
            model.IsActive = true;
        };

        Closed += (_, _) =>
        {
            model.IsActive = false;
        };
    }
}