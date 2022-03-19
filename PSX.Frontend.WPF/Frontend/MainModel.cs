using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Core;
using PSX.Frontend.WPF.Emulation;
using PSX.Frontend.WPF.Emulation.Messaging;
using PSX.Frontend.WPF.Frontend.Services;
using PSX.Frontend.WPF.Frontend.Shared;
using PSX.Frontend.WPF.Logging;

namespace PSX.Frontend.WPF.Frontend;

internal sealed class MainModel :
    BaseModel<MainModelCommands>,
    IRecipient<BaseUpdateMessage<UpdateAudioDataMessageHandler>>,
    IRecipient<BaseUpdateMessage<UpdateVideoDataMessageHandler>>,
    IRecipient<BaseUpdateMessage<UpdateVideoSizeMessageHandler>>
{
    private Emulator? Emulator { get; set; }

    private CancellationTokenSource? EmulatorTokenSource { get; set; }

    private string? EmulatorContent { get; set; }

    private bool EmulatorPaused { get; set; }

    public bool CanContinue => Emulator is not null && EmulatorPaused;

    public bool CanOpen => true;

    public bool CanPause => Emulator is not null && EmulatorPaused is false;

    public bool CanStart => Emulator is null && EmulatorContent is not null;

    public bool CanTerminate => Emulator is not null;

    public void Open()
    {
        var service = App.Current.Services.GetService<IFilePickerService>() ?? throw new InvalidOperationException();

        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = service.OpenFile(filter: filter);

        if (path is null)
            return;

        EmulatorContent = path;
    }

    public void Start()
    {
        if (Emulator is not null)
            throw new InvalidOperationException();

        if (EmulatorContent is null)
            throw new InvalidOperationException();

        var window = new HostWindow
        {
            UpdateAudioDataHandler = UpdateAudioData,
            UpdateVideoDataHandler = UpdateVideoData,
            UpdateVideoSizeHandler = UpdateVideoSize
        };

        Emulator = new Emulator(window, EmulatorContent, LoggingUtility.GetDefaultLogger());

        EmulatorPaused = false;

        EmulatorTokenSource?.Dispose();

        EmulatorTokenSource = new CancellationTokenSource();

        var token = EmulatorTokenSource.Token;

        Task.Factory.StartNew(() => UpdateLoop(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    public void Pause()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        EmulatorPaused = true;
    }

    public void Continue()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        EmulatorPaused = false;
    }

    public void Terminate()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        if (EmulatorTokenSource is null)
            throw new InvalidOperationException();

        EmulatorPaused = true;

        EmulatorTokenSource.Cancel();

        Emulator.Dispose();

        Emulator = null;
    }

    private void UpdateLoop(CancellationToken token)
    {
        try
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                if (EmulatorPaused)
                {
                    // TODO use some Thread.Suspend or whatever
                }
                else
                {
                    Emulator?.RunFrame();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e); // TODO better logging mechanism
        }
    }

    #region Update handlers

    private UpdateAudioDataMessage? LastUpdateAudioDataMessage { get; set; }

    private UpdateVideoDataMessage? LastUpdateVideoDataMessage { get; set; }

    private UpdateVideoSizeMessage? LastUpdateVideoSizeMessage { get; set; }

    private ISet<UpdateVideoSizeMessageHandler> UpdateVideoSizeHandlers { get; } = new HashSet<UpdateVideoSizeMessageHandler>();

    private ISet<UpdateAudioDataMessageHandler> UpdateAudioDataHandlers { get; } = new HashSet<UpdateAudioDataMessageHandler>();

    private ISet<UpdateVideoDataMessageHandler> UpdateVideoDataHandlers { get; } = new HashSet<UpdateVideoDataMessageHandler>();

    void IRecipient<BaseUpdateMessage<UpdateAudioDataMessageHandler>>.Receive(BaseUpdateMessage<UpdateAudioDataMessageHandler> message)
    {
        switch (message.Type)
        {
            case BaseUpdateMessageType.Add:
                UpdateAudioDataHandlers.Add(message.Handler);
                break;
            case BaseUpdateMessageType.Remove:
                UpdateAudioDataHandlers.Remove(message.Handler);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (message.Type is BaseUpdateMessageType.Add && LastUpdateAudioDataMessage != null)
            UpdateAudioData(LastUpdateAudioDataMessage);
    }

    void IRecipient<BaseUpdateMessage<UpdateVideoDataMessageHandler>>.Receive(BaseUpdateMessage<UpdateVideoDataMessageHandler> message)
    {
        switch (message.Type)
        {
            case BaseUpdateMessageType.Add:
                UpdateVideoDataHandlers.Add(message.Handler);
                break;
            case BaseUpdateMessageType.Remove:
                UpdateVideoDataHandlers.Remove(message.Handler);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (message.Type is BaseUpdateMessageType.Add && LastUpdateVideoDataMessage != null)
            UpdateVideoData(LastUpdateVideoDataMessage);
    }

    void IRecipient<BaseUpdateMessage<UpdateVideoSizeMessageHandler>>.Receive(BaseUpdateMessage<UpdateVideoSizeMessageHandler> message)
    {
        switch (message.Type)
        {
            case BaseUpdateMessageType.Add:
                UpdateVideoSizeHandlers.Add(message.Handler);
                break;
            case BaseUpdateMessageType.Remove:
                UpdateVideoSizeHandlers.Remove(message.Handler);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (message.Type is BaseUpdateMessageType.Add && LastUpdateVideoSizeMessage != null)
            UpdateVideoSize(LastUpdateVideoSizeMessage);
    }

    private void UpdateAudioData(UpdateAudioDataMessage message)
    {
        LastUpdateAudioDataMessage = message;

        foreach (var handler in UpdateAudioDataHandlers)
            handler(message);
    }

    private void UpdateVideoData(UpdateVideoDataMessage message)
    {
        LastUpdateVideoDataMessage = message;

        foreach (var handler in UpdateVideoDataHandlers)
            handler(message);
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        LastUpdateVideoSizeMessage = message;

        foreach (var handler in UpdateVideoSizeHandlers)
            handler(message);
    }

    #endregion
}