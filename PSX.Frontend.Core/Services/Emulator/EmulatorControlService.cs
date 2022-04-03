using System.Diagnostics;

namespace PSX.Frontend.Core.Services.Emulator;

internal sealed class EmulatorControlService : IEmulatorControlService
{
    public EmulatorControlService(IEmulatorDisplayService emulatorDisplayService)
    {
        EmulatorDisplayService = emulatorDisplayService ?? throw new ArgumentNullException(nameof(emulatorDisplayService));
    }

    private PSX.Emulator? Emulator { get; set; }

    private IEmulatorDisplayService EmulatorDisplayService { get; }

    private EmulatorPlayerState EmulatorState { get; set; }

    private CancellationTokenSource EmulatorTokenSource { get; set; } = new();

    private void UpdateLoop(object? cts)
    {
        if (cts is not CancellationToken token)
            throw new ArgumentOutOfRangeException(nameof(cts));

        var span = TimeSpan.FromSeconds(1.0d / 60.0d);
        var zero = TimeSpan.Zero;

        var stopwatch = new Stopwatch();

        while (true)
        {
            if (token.IsCancellationRequested)
                break;

            if (EmulatorState == EmulatorPlayerState.Pausing)
            {
                Thread.Sleep(span);
                continue;
            }

            if (EmulatorState is EmulatorPlayerState.Running or EmulatorPlayerState.Framing)
            {
                stopwatch.Restart();

                Emulator?.RunFrame();

                var frame = stopwatch.Elapsed;
                var sleep = span - frame;

                if (sleep > zero)
                {
                    Thread.Sleep(sleep);
                }

                if (EmulatorState is EmulatorPlayerState.Framing)
                {
                    EmulatorState = EmulatorPlayerState.Pausing;
                }
            }
        }
    }

    private enum EmulatorPlayerState
    {
        Stopped,
        Running,
        Pausing,
        Framing
    }

    #region IEmulatorControlService

    public bool CanStart => Emulator is not null && EmulatorState is EmulatorPlayerState.Stopped; // TODO this should account that content is set

    public bool CanStop => Emulator is not null && EmulatorState is EmulatorPlayerState.Running;

    public bool CanPause => Emulator is not null && EmulatorState is EmulatorPlayerState.Running;

    public bool CanContinue => Emulator is not null && EmulatorState is EmulatorPlayerState.Pausing;

    public bool CanFrame => Emulator is not null && EmulatorState is EmulatorPlayerState.Running or EmulatorPlayerState.Pausing;

    public void Setup(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(content));

        EmulatorState = EmulatorPlayerState.Stopped;

        if (Emulator is not null)
        {
            EmulatorTokenSource.Cancel();

            Emulator.Dispose();

            Emulator = null;
        }

        Emulator = new PSX.Emulator(EmulatorDisplayService, content);
    }

    public void Start()
    {
        if (CanStart is false)
            throw new InvalidOperationException();

        EmulatorState = EmulatorPlayerState.Running;

        EmulatorTokenSource.Dispose();

        EmulatorTokenSource = new CancellationTokenSource();

        var token = EmulatorTokenSource.Token;

        Task.Factory.StartNew(UpdateLoop, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    public void Stop()
    {
        if (CanStop is false)
            throw new InvalidOperationException();

        EmulatorState = EmulatorPlayerState.Stopped;
    }

    public void Pause()
    {
        if (CanPause is false)
            throw new InvalidOperationException();

        EmulatorState = EmulatorPlayerState.Pausing;
    }

    public void Continue()
    {
        if (CanContinue is false)
            throw new InvalidOperationException();

        EmulatorState = EmulatorPlayerState.Running;
    }

    public void Frame()
    {
        if (CanFrame is false)
            throw new InvalidOperationException();

        EmulatorState = EmulatorPlayerState.Framing;
    }

    #endregion
}