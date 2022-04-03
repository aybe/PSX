using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Frontend.Core.Services.Emulator;

namespace PSX.Frontend.Core.Interface;

public sealed class VideoScreenViewModel : ObservableRecipient
{
    public VideoScreenViewModel(VideoScreenModel model, IEmulatorService emulatorService)
    {
        Model           = model;
        EmulatorService = emulatorService;
    }

    private VideoScreenModel Model { get; }

    private IEmulatorService EmulatorService { get; }

    protected override void OnActivated()
    {
        base.OnActivated();

        EmulatorService.UpdateVideoDataMessageHandlers.Add(UpdateVideoData);
        EmulatorService.UpdateVideoSizeMessageHandlers.Add(UpdateVideoSize);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        EmulatorService.UpdateVideoDataMessageHandlers.Remove(UpdateVideoData);
        EmulatorService.UpdateVideoSizeMessageHandlers.Remove(UpdateVideoSize);
    }

    private void UpdateVideoData(UpdateVideoDataMessage message)
    {
        throw new NotImplementedException();
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        throw new NotImplementedException();
    }
}