namespace PSX.Frontend.Core.Services.Emulator;

public sealed class EmulatorUpdate : IEmulatorUpdate
{
    private ushort DisplayVRamXStart { get; set; }

    private ushort DisplayVRamYStart { get; set; }

    private ushort DisplayX1 { get; set; }

    private ushort DisplayX2 { get; set; }

    private ushort DisplayY2 { get; set; }

    private ushort DisplayY1 { get; set; }

    [Obsolete]
    public EmulatorUpdateAudioDataMessageHandler? UpdateAudioDataHandler { get; set; }

    [Obsolete]
    public EmulatorUpdateVideoDataMessageHandler? UpdateVideoDataHandler { get; set; }

    [Obsolete]
    public EmulatorUpdateVideoSizeMessageHandler? UpdateVideoSizeHandler { get; set; }

    public IList<EmulatorUpdateAudioDataMessageHandler> UpdateAudioDataMessageHandlers { get; } = new List<EmulatorUpdateAudioDataMessageHandler>();

    public IList<EmulatorUpdateVideoDataMessageHandler> UpdateVideoDataMessageHandlers { get; } = new List<EmulatorUpdateVideoDataMessageHandler>();

    public IList<EmulatorUpdateVideoSizeMessageHandler> UpdateVideoSizeMessageHandlers { get; } = new List<EmulatorUpdateVideoSizeMessageHandler>();

    public void Play(byte[] samples)
    {
        if (UpdateAudioDataHandler is null)
            return;

        var message = new EmulatorUpdateAudioDataMessage(samples);

        UpdateAudioDataHandler(message);

        foreach (var handler in UpdateAudioDataMessageHandlers)
        {
            handler(message);
        }
    }

    public void Render(int[] buffer24, ushort[] buffer16)
    {
        if (UpdateVideoDataHandler is null)
            return;

        var size = new IntSize(DisplayVRamXStart, DisplayVRamYStart);
        var rect = new IntRect(DisplayX1, DisplayY1, DisplayX2, DisplayY2);

        var message = new EmulatorUpdateVideoDataMessage(size, rect, buffer24, buffer16);

        UpdateVideoDataHandler(message);

        foreach (var handler in UpdateVideoDataMessageHandlers)
        {
            handler(message);
        }
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
        if (UpdateVideoSizeHandler is null)
            return;

        var message = new EmulatorUpdateVideoSizeMessage(new IntSize(horizontalRes, verticalRes), is24BitDepth);

        UpdateVideoSizeHandler(message);

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
}