using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Un4seen.Bass;

namespace ProjectPSX.WPF;

internal sealed class WpfHostWindow : IHostWindow
{
  
    private ushort DisplayVRamXStart { get; set; }

    private ushort DisplayVRamYStart { get; set; }

    private ushort DisplayX1 { get; set; }

    private ushort DisplayX2 { get; set; }

    private ushort DisplayY2 { get; set; }

    private ushort DisplayY1 { get; set; }

    public int StreamPush { get; set; }

    public int StreamMixer { get; set; }
    
    public UpdateBitmapSizeHandler UpdateBitmapSize { get; set; }

    public UpdateBitmapDataHandler UpdateBitmapData { get; set; }

    public  void Render(int[] vram)
    {
        var size = new IntSize(DisplayVRamXStart, DisplayVRamYStart);
        var rect = new IntRect(DisplayX1, DisplayY1, DisplayX2, DisplayY2);
        UpdateBitmapData(size, rect, vram);
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
         UpdateBitmapSize(new IntSize(horizontalRes, verticalRes), is24BitDepth);
    }

    public void SetVRAMStart(ushort displayVRAMXStart, ushort displayVRAMYStart)
    {
        DisplayVRamXStart = displayVRAMXStart;
        DisplayVRamYStart = displayVRAMYStart;
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

    public void Play(byte[] samples)
    {
        var data = Bass.BASS_StreamPutData(StreamPush, samples, samples.Length);
        if (data is -1)
        {
            throw new BassException();
        }
    }
}

public readonly struct IntRect
{
    public int X1 { get; }

    public int Y1 { get; }

    public int X2 { get; }

    public int Y2 { get; }

    public IntRect(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public override string ToString()
    {
        return $"{nameof(X1)}: {X1}, {nameof(Y1)}: {Y1}, {nameof(X2)}: {X2}, {nameof(Y2)}: {Y2}";
    }
}

public readonly struct IntSize
{
    public int X { get; }

    public int Y { get; }

    public IntSize(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public delegate void UpdateBitmapDataHandler(IntSize size, IntRect rect, int[] pixels);

public delegate void UpdateBitmapSizeHandler(IntSize size, bool bpp24);