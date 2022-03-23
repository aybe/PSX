using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Core;
using PSX.Frontend.WPF.Emulation;
using PSX.Frontend.WPF.Frontend.Shared;
using PSX.Frontend.WPF.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PSX.Frontend.WPF.Frontend;

internal sealed class MainModel : ObservableRecipient
{
    public MainModel()
    {
        OpenContent         = new RelayCommand(OpenContentExecute,         () => true);
        EmulationStart      = new RelayCommand(EmulationStartExecute,      () => CanStart);
        EmulationPause      = new RelayCommand(EmulationPauseExecute,      () => CanPause);
        EmulationFrame      = new RelayCommand(EmulationFrameExecute,      () => CanFrame);
        EmulationContinue   = new RelayCommand(EmulationContinueExecute,   () => CanContinue);
        EmulationTerminate  = new RelayCommand(EmulationTerminateExecute,  () => CanTerminate);
        ApplicationShutdown = new RelayCommand(ApplicationShutdownExecute, () => true);
    }

    private Emulator? Emulator { get; set; }

    private CancellationTokenSource? EmulatorTokenSource { get; set; }

    private string? EmulatorContent { get; set; }

    private bool EmulatorFrame { get; set; }

    private bool EmulatorPaused { get; set; }

    private bool CanContinue => Emulator is not null && EmulatorPaused;

    private bool CanFrame => Emulator is not null;

    private bool CanPause => Emulator is not null && EmulatorPaused is false;

    private bool CanStart => Emulator is null && EmulatorContent is not null;

    private bool CanTerminate => Emulator is not null;

    [SuppressMessage("ReSharper", "InvertIf")]
    private void UpdateLoop(CancellationToken token)
    {
        var logger = Log.ForContext<MainModel>();

        var span = TimeSpan.FromSeconds(1.0d / 60.0d);
        var zero = TimeSpan.Zero;

        var stopwatch = new Stopwatch();

        while (true)
        {
            if (token.IsCancellationRequested)
                break;

            if (EmulatorPaused)
            {
                Thread.Sleep(span);
            }
            else
            {
                if (EmulatorFrame)
                {
                    EmulatorPaused = true;
                }

                stopwatch.Restart();

                Emulator?.RunFrame();

                var frame = stopwatch.Elapsed;

                logger?.Write(frame > span ? LogEventLevel.Error : LogEventLevel.Debug, "Time spent FRAME: {Time}", frame);

                var sleep = span - frame;

                if (sleep > zero)
                {
                    Thread.Sleep(sleep);
                    logger?.Write(LogEventLevel.Debug, "Time spent SLEEP: {Time}", sleep);
                }
            }
        }
    }

    #region Commands

    public RelayCommand OpenContent { get; }

    private void OpenContentExecute()
    {
        var service = App.Current.Services.GetService<IFilePickerService>() ?? throw new InvalidOperationException();

        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = service.OpenFile(filter: filter);

        if (path is null)
            return;

        EmulatorContent = path;
    }

    public RelayCommand EmulationStart { get; }

    private void EmulationStartExecute()
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

        Emulator = new Emulator(window, EmulatorContent);

        EmulatorPaused = false;

        EmulatorTokenSource?.Dispose();

        EmulatorTokenSource = new CancellationTokenSource();

        var token = EmulatorTokenSource.Token;

        Task.Factory.StartNew(() => UpdateLoop(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    public RelayCommand EmulationPause { get; }

    private void EmulationPauseExecute()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        EmulatorPaused = true;
    }

    public RelayCommand EmulationFrame { get; }

    private void EmulationFrameExecute()
    {
        EmulatorFrame = true;

        if (EmulatorPaused)
        {
            EmulatorPaused = false;
        }
    }

    public RelayCommand EmulationContinue { get; }

    private void EmulationContinueExecute()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        EmulatorPaused = false;

        if (EmulatorFrame)
        {
            EmulatorFrame = false;
        }
    }

    public RelayCommand EmulationTerminate { get; }

    private void EmulationTerminateExecute()
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

    public RelayCommand ApplicationShutdown { get; }

    private void ApplicationShutdownExecute()
    {
        var service = App.Current.Services.GetService<IApplication>() ?? throw new InvalidOperationException();
        service.Shutdown();
    }

    #endregion
}