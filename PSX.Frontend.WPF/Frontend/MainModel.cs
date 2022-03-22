using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Core;
using PSX.Frontend.WPF.Emulation;
using PSX.Frontend.WPF.Frontend.Services;
using PSX.Frontend.WPF.Logging;

namespace PSX.Frontend.WPF.Frontend;

internal sealed class MainModel : ObservableRecipient
{
    public MainModel()
    {
        Commands = new MainModelCommands(this);
    }

    public MainModelCommands Commands { get; }

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
            UpdateAudioDataHandler = message => WeakReferenceMessenger.Default.Send(message),
            UpdateVideoDataHandler = message => WeakReferenceMessenger.Default.Send(message),
            UpdateVideoSizeHandler = message => WeakReferenceMessenger.Default.Send(message)
        };

        LoggingUtility.Initialize();

        Emulator = new Emulator(window, EmulatorContent);

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
}