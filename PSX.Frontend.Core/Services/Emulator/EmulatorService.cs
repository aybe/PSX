using System.Diagnostics;

namespace PSX.Frontend.Core.Services.Emulator;

public sealed class EmulatorService : IEmulatorService
{
    private PSX.Emulator? Emulator { get; set; }

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

    #region IEmulatorService

    public bool CanStart => Emulator is not null && EmulatorState is EmulatorPlayerState.Stopped; // TODO this should account that content is set

    public bool CanStop => Emulator is not null && EmulatorState is EmulatorPlayerState.Running;

    public bool CanPause => Emulator is not null && EmulatorState is EmulatorPlayerState.Running;

    public bool CanContinue => Emulator is not null && EmulatorState is EmulatorPlayerState.Pausing;

    public bool CanFrame => Emulator is not null && EmulatorState is EmulatorPlayerState.Running or EmulatorPlayerState.Pausing;

    public IList<UpdateAudioDataMessageHandler> UpdateAudioDataMessageHandlers { get; } = new List<UpdateAudioDataMessageHandler>();

    public IList<UpdateVideoDataMessageHandler> UpdateVideoDataMessageHandlers { get; } = new List<UpdateVideoDataMessageHandler>();

    public IList<UpdateVideoSizeMessageHandler> UpdateVideoSizeMessageHandlers { get; } = new List<UpdateVideoSizeMessageHandler>();

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

        Emulator = new PSX.Emulator(this, content);
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

    #region IHostWindow

    private ushort DisplayVRamXStart { get; set; }

    private ushort DisplayVRamYStart { get; set; }

    private ushort DisplayX1 { get; set; }

    private ushort DisplayX2 { get; set; }

    private ushort DisplayY2 { get; set; }

    private ushort DisplayY1 { get; set; }

    public void Play(byte[] samples)
    {
        var message = new UpdateAudioDataMessage(samples);

        foreach (var handler in UpdateAudioDataMessageHandlers)
        {
            handler(message);
        }
    }

    public void Render(int[] buffer24, ushort[] buffer16)
    {
        var size = new IntSize(DisplayVRamXStart, DisplayVRamYStart);
        var rect = new IntRect(DisplayX1, DisplayY1, DisplayX2, DisplayY2);

        var message = new UpdateVideoDataMessage(size, rect, buffer24, buffer16);

        foreach (var handler in UpdateVideoDataMessageHandlers)
        {
            handler(message);
        }
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
        var message = new UpdateVideoSizeMessage(new IntSize(horizontalRes, verticalRes), is24BitDepth);

        foreach (var handler in UpdateVideoSizeMessageHandlers)
        {
            handler(message);
        }
    }

    public void SetHorizontalRange(ushort displayX1, ushort displayX2)
    {
        DisplayX1 = displayX1;
        DisplayX2 = displayX2;
    }

    public void SetVerticalRange(ushort displayY1, ushort displayY2)
    {
        DisplayY1 = displayY1;
        DisplayY2 = displayY2;
    }

    public void SetVRAMStart(ushort displayVRamStartX, ushort displayVRamStartY)
    {
        DisplayVRamXStart = displayVRamStartX;
        DisplayVRamYStart = displayVRamStartY;
    }

    #endregion
}