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

    public void Render(int[] buffer24, ushort[] buffer16)
    {
        var size = new IntSize(DisplayVRamXStart, DisplayVRamYStart);
        var rect = new IntRect(DisplayX1, DisplayY1, DisplayX2, DisplayY2);
        UpdateBitmapData(size, rect, buffer24, buffer16);
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