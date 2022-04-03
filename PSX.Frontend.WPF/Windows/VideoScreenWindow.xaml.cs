using PSX.Frontend.Interface;

namespace PSX.Frontend.WPF.Windows;

public partial class VideoScreenWindow : IVideoScreenView
{
    public VideoScreenWindow(VideoScreenViewModel model)
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