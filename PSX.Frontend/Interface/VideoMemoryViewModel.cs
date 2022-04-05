using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Messages;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class VideoMemoryViewModel : ObservableRecipient
{
    public VideoMemoryViewModel(VideoMemoryModel model, IEmulatorDisplayService emulatorDisplayService)
    {
        Model                  = model;
        EmulatorDisplayService = emulatorDisplayService;
    }

    private VideoMemoryModel Model { get; }

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

    private void UpdateVideoData(UpdateVideoDataMessage message)
        // TODO reuse
    {
        var msg = new UpdateBitmapMessage(
            message.Size.Width,
            message.Size.Height,
            message.Rect.XMin,
            message.Rect.XMax,
            message.Rect.YMin,
            message.Rect.YMax,
            message.Buffer16,
            message.Buffer24
        );

        WeakReferenceMessenger.Default.Send(msg);
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
        // TODO reuse
    {
        var msg = new CreateBitmapMessage(
            message.Size.Width,
            message.Size.Height,
            message.Is24Bit ? CreateBitmapFormat.Direct24 : CreateBitmapFormat.Direct15
        );

        WeakReferenceMessenger.Default.Send(msg);
    }
}