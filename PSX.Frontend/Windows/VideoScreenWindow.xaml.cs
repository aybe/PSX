using PSX.Frontend.Core.Interface;

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