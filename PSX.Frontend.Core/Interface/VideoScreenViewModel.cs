using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.Core.Interface;

public sealed class VideoScreenViewModel : ObservableRecipient
{
    public VideoScreenViewModel(VideoScreenModel model)
    {
        Model = model;
    }

    private VideoScreenModel Model { get; }
}