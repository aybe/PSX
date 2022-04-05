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

        EmulatorDisplayService.UpdateVideoDataHandlers.Add(UpdateVideoData);
        EmulatorDisplayService.UpdateVideoSizeHandlers.Add(UpdateVideoSize);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        EmulatorDisplayService.UpdateVideoDataHandlers.Remove(UpdateVideoData);
        EmulatorDisplayService.UpdateVideoSizeHandlers.Remove(UpdateVideoSize);
    }

    private static void UpdateVideoData(UpdateVideoData data)
    {
        WeakReferenceMessenger.Default.Send(data);
    }

    private static void UpdateVideoSize(UpdateVideoSize size)
    {
        WeakReferenceMessenger.Default.Send(size);
    }
}