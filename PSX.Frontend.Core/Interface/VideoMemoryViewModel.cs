using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.Core.Interface;

public sealed class VideoMemoryViewModel : ObservableRecipient
{
    public VideoMemoryViewModel(VideoMemoryModel model)
    {
        Model = model;
    }

    private VideoMemoryModel Model { get; }
}