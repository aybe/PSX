namespace PSX.Frontend.Services.Emulation;

public sealed class EmulatorDisplayService : IEmulatorDisplayService
{
    #region IHostWindow

    private ushort DisplayVRamXStart { get; set; }

    private ushort DisplayVRamYStart { get; set; }

    private ushort DisplayX1 { get; set; }

    private ushort DisplayX2 { get; set; }

    private ushort DisplayY2 { get; set; }

    private ushort DisplayY1 { get; set; }

    public void SetDisplayAreaStart(ushort displayVRamStartX, ushort displayVRamStartY)
    {
        DisplayVRamXStart = displayVRamStartX;
        DisplayVRamYStart = displayVRamStartY;
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
        var size = new UpdateVideoSize(
            horizontalRes,
            verticalRes,
            is24BitDepth ? UpdateVideoSizeFormat.Direct24 : UpdateVideoSizeFormat.Direct15
        );

        foreach (var handler in UpdateVideoSizeHandlers)
        {
            handler(size);
        }
    }

    public void SetDisplayHorizontalRange(ushort displayX1, ushort displayX2)
    {
        DisplayX1 = displayX1;
        DisplayX2 = displayX2;
    }

    public void SetDisplayVerticalRange(ushort displayY1, ushort displayY2)
    {
        DisplayY1 = displayY1;
        DisplayY2 = displayY2;
    }

    public void Render(int[] buffer24, ushort[] buffer16)
    {
        var data = new UpdateVideoData(
            DisplayVRamXStart,
            DisplayVRamYStart,
            DisplayX1,
            DisplayX2,
            DisplayY1,
            DisplayY2,
            buffer16,
            buffer24
        );

        foreach (var handler in UpdateVideoDataHandlers)
        {
            handler(data);
        }
    }

    public void Play(byte[] samples)
    {
        var message = new UpdateAudioDataMessage(samples);

        foreach (var handler in UpdateAudioDataHandlers)
        {
            handler(message);
        }
    }

    #endregion

    #region IEmulatorDisplayService

    public IList<UpdateAudioDataHandler> UpdateAudioDataHandlers { get; } = new List<UpdateAudioDataHandler>();

    public IList<UpdateVideoDataHandler> UpdateVideoDataHandlers { get; } = new List<UpdateVideoDataHandler>();

    public IList<UpdateVideoSizeHandler> UpdateVideoSizeHandlers { get; } = new List<UpdateVideoSizeHandler>();

    #endregion
}