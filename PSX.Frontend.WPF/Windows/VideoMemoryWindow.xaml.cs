using PSX.Frontend.Core.Interface;

namespace PSX.Frontend.WPF.Windows;

public partial class VideoMemoryWindow : IVideoMemoryView
{
    public VideoMemoryWindow(VideoMemoryViewModel model)
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