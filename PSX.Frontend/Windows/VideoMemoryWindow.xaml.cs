using PSX.Frontend.Core.ViewModels;
using PSX.Frontend.Core.Views;

namespace PSX.Frontend.Windows;

public partial class VideoMemoryWindow : IVideoMemoryView
{
    public VideoMemoryWindow(ViewMemoryViewModel model)
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