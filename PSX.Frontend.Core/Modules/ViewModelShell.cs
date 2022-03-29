﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Core;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.Core.Services;
using PSX.Logging;
using Serilog;
using Serilog.Events;

namespace PSX.Frontend.Core.Modules;

public sealed class ViewModelShell : ObservableRecipient, IObservableLog
{
    public ViewModelShell(IOptions<AppSettings> options, ILogger<ViewModelShell> logger, IServiceProvider serviceProvider, EmulatorUpdate update, INavigationService navigationService)
    {
        Options           = options;
        Logger            = logger;
        EmulatorUpdate    = update ?? throw new ArgumentNullException(nameof(update));
        NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        var service = serviceProvider.GetService<ILoggerProvider>();

        if (service is IObservableLog log)
        {
            Entries = log.Entries;
        }

        LogSomething = new RelayCommand(() =>
        {
            Logger.LogInformation("From MainViewModel: {Time}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        });

        OpenContent         = new RelayCommand(OpenContentExecute,         () => true);
        OpenLog             = new RelayCommand(OpenLogExecute,             () => true);
        EmulationStart      = new RelayCommand(EmulationStartExecute,      () => CanStart);
        EmulationPause      = new RelayCommand(EmulationPauseExecute,      () => CanPause);
        EmulationFrame      = new RelayCommand(EmulationFrameExecute,      () => CanFrame);
        EmulationContinue   = new RelayCommand(EmulationContinueExecute,   () => CanContinue);
        EmulationTerminate  = new RelayCommand(EmulationTerminateExecute,  () => CanTerminate);
        ApplicationShutdown = new RelayCommand(ApplicationShutdownExecute, () => true);
    }

    public ICommand LogSomething { get; }

    public IOptions<AppSettings> Options { get; }

    public ILogger<ViewModelShell> Logger { get; }

    public EmulatorUpdate EmulatorUpdate { get; }

    public INavigationService NavigationService { get; }

    public string SomethingFromAppSettings => $"{Options.Value.Executable} from VM";

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

    public ObservableCollection<LogEntry>? Entries { get; }

    private void RefreshEmulatorCommands()
    {
        static void NotifyCanExecuteChanged(params RelayCommand[] commands)
        {
            if (commands is null)
                throw new ArgumentNullException(nameof(commands));

            foreach (var command in commands)
            {
                command.NotifyCanExecuteChanged();
            }
        }

        NotifyCanExecuteChanged(EmulationStart, EmulationPause, EmulationContinue, EmulationFrame, EmulationTerminate);
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    private void UpdateLoop(CancellationToken token)
    {
        var logger = Log.ForContext<ViewModelShell>();

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
        var service = AppStartup.Current.Host.Services.GetRequiredService<IOpenFileService>() ?? throw new InvalidOperationException();

        const string filter = "Everything|*.exe;*.psx;*.bin;*.cue|Application|*.exe;*.psx|Image|*.bin;*.cue";

        var path = service.OpenFile(filter: filter);

        if (path is null)
            return;

        EmulatorContent = path;

        RefreshEmulatorCommands();
    }

    public RelayCommand OpenLog { get; }

    private void OpenLogExecute()
    {
        NavigationService.Navigate<IViewLog>();
    }

    public RelayCommand EmulationStart { get; }

    private void EmulationStartExecute()
    {
        if (Emulator is not null)
            throw new InvalidOperationException();

        if (EmulatorContent is null)
            throw new InvalidOperationException();

        EmulatorUpdate.UpdateAudioDataHandler = message => WeakReferenceMessenger.Default.Send(message);
        EmulatorUpdate.UpdateVideoDataHandler = message => WeakReferenceMessenger.Default.Send(message);
        EmulatorUpdate.UpdateVideoSizeHandler = message => WeakReferenceMessenger.Default.Send(message);

        Emulator = new Emulator(EmulatorUpdate, EmulatorContent);

        EmulatorPaused = false;

        EmulatorTokenSource?.Dispose();

        EmulatorTokenSource = new CancellationTokenSource();

        var token = EmulatorTokenSource.Token;

        Task.Factory.StartNew(() => UpdateLoop(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        RefreshEmulatorCommands();
    }

    public RelayCommand EmulationPause { get; }

    private void EmulationPauseExecute()
    {
        if (Emulator is null)
            throw new InvalidOperationException();

        EmulatorPaused = true;

        RefreshEmulatorCommands();
    }

    public RelayCommand EmulationFrame { get; }

    private void EmulationFrameExecute()
    {
        EmulatorFrame = true;

        if (EmulatorPaused)
        {
            EmulatorPaused = false;
        }

        RefreshEmulatorCommands();
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

        RefreshEmulatorCommands();
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

        RefreshEmulatorCommands();
    }

    public RelayCommand ApplicationShutdown { get; }

    private void ApplicationShutdownExecute()
    {
        AppStartup.Current.Host.Services.GetRequiredService<IApplicationService>().Shutdown();
    }

    #endregion
}