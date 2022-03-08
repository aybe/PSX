using System;

namespace ProjectPSX.WPF.Emulation;

internal sealed class HostWindow : IHostWindow
{
    private ushort DisplayVRamXStart { get; set; }

    private ushort DisplayVRamYStart { get; set; }

    private ushort DisplayX1 { get; set; }

    private ushort DisplayX2 { get; set; }

    private ushort DisplayY2 { get; set; }

    private ushort DisplayY1 { get; set; }

    public UpdateBitmapSize? UpdateBitmapSize { get; init; }

    public UpdateBitmapData? UpdateBitmapData { get; init; }

    public UpdateSampleData? UpdateSampleData { get; init; }

    public void Play(byte[] samples)
    {
        if (UpdateSampleData is null)
            throw new NullReferenceException(nameof(UpdateSampleData));

        UpdateSampleData(samples);
    }

    public void Render(int[] buffer24, ushort[] buffer16)
    {
        if (UpdateBitmapData is null)
            throw new NullReferenceException(nameof(UpdateBitmapData));

        var size = new IntSize(DisplayVRamXStart, DisplayVRamYStart);
        var rect = new IntRect(DisplayX1, DisplayY1, DisplayX2, DisplayY2);
        UpdateBitmapData(size, rect, buffer24, buffer16);
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
        if (UpdateBitmapSize is null)
            throw new NullReferenceException(nameof(UpdateBitmapSize));

        UpdateBitmapSize(new IntSize(horizontalRes, verticalRes), is24BitDepth);
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