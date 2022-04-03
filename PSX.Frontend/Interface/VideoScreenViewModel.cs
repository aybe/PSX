﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.Interface;

public sealed class VideoScreenViewModel : ObservableRecipient
{
    public VideoScreenViewModel(VideoScreenModel model, IEmulatorDisplayService emulatorDisplayService)
    {
        Model                  = model;
        EmulatorDisplayService = emulatorDisplayService;
    }

    private VideoScreenModel Model { get; }

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
    {
        // TODO
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        // TODO
    }
}