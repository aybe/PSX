namespace PSX.Frontend.Core.Services.Emulator;

public sealed class EmulatorDisplayService : IEmulatorDisplayService
{
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

    #region IEmulatorDisplayService

    public IList<UpdateAudioDataMessageHandler> UpdateAudioDataMessageHandlers { get; } = new List<UpdateAudioDataMessageHandler>();

    public IList<UpdateVideoDataMessageHandler> UpdateVideoDataMessageHandlers { get; } = new List<UpdateVideoDataMessageHandler>();

    public IList<UpdateVideoSizeMessageHandler> UpdateVideoSizeMessageHandlers { get; } = new List<UpdateVideoSizeMessageHandler>();

    #endregion
}