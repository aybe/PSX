using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class VideoViewModel : ObservableRecipient
{
    public VideoViewModel(IEmulatorDisplayService emulatorDisplayService)
    {
        EmulatorDisplayService = emulatorDisplayService;
    }

    private IEmulatorDisplayService EmulatorDisplayService { get; }

    protected override void OnActivated()
    {
        base.OnActivated();

        EmulatorDisplayService.UpdateVideoDataMessageHandlers.Add(UpdateVideoData);
        EmulatorDisplayService.UpdateVideoSizeMessageHandlers.Add(UpdateVideoSize);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        EmulatorDisplayService.UpdateVideoDataMessageHandlers.Remove(UpdateVideoData);
        EmulatorDisplayService.UpdateVideoSizeMessageHandlers.Remove(UpdateVideoSize);
    }

    private static void UpdateVideoData(UpdateVideoDataMessage message)
    {
        WeakReferenceMessenger.Default.Send(message);
    }

    private static void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        WeakReferenceMessenger.Default.Send(message);
    }
}