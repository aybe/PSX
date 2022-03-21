using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using NAudio.Wave;
using PSX.Core;
using PSX.Core.Interfaces;
using PSX.Devices.Input;
using PSX.Frontend.WinForms.Interop;
using Timer = System.Timers.Timer;

namespace PSX.Frontend.WinForms.UI;

public class Window : Form, IHostWindow
{
    private const int PsxMhz         = 33868800;
    private const int SyncCycles     = 100;
    private const int MipsUnderclock = 3;

    private const    int  CyclesPerFrame = PsxMhz / 60;
    private const    int  SyncLoops      = CyclesPerFrame / (SyncCycles * MipsUnderclock) + 1;
    private const    int  Cycles         = SyncLoops * SyncCycles;
    private readonly Size _640x480       = new(640, 480);

    private readonly Dictionary<Keys, KeyboardInput> _gamepadKeyMap;
    private readonly BufferedWaveProvider            BufferedWaveProvider = new(new WaveFormat());

    private readonly GdiBitmap Display = new(1024, 512);

    private readonly Emulator            Psx;
    private readonly DoubleBufferedPanel Screen = new();

    private readonly Size VRAMSize = new(1024, 512);

    private readonly WaveOutEvent WaveOutEvent = new();

    private long CpuCyclesCounter;

    private int DisplayVramxStart;
    private int DisplayVramyStart;

    private int DisplayX1;
    private int DisplayX2;
    private int DisplayY1;
    private int DisplayY2;
    private int Fps;

    private int HorizontalRes;

    private bool Is24BitDepth;
    private bool IsVramViewer;
    private int  VerticalRes;

    public Window()
    {
        Text            =  "ProjectPSX";
        AutoSize        =  true;
        AutoSizeMode    =  AutoSizeMode.GrowAndShrink;
        FormBorderStyle =  FormBorderStyle.FixedDialog;
        KeyUp           += VRAMViewerToggle;

        Screen.Size             =  _640x480;
        Screen.Margin           =  new Padding(0);
        Screen.MouseDoubleClick += ToggleDebug;

        Controls.Add(Screen);

        KeyDown += HandleJoyPadDown;
        KeyUp   += HandleJoyPadUp;

        _gamepadKeyMap = new Dictionary<Keys, KeyboardInput>
        {
            { Keys.Space, KeyboardInput.Space },
            { Keys.Z, KeyboardInput.Z },
            { Keys.C, KeyboardInput.C },
            { Keys.Enter, KeyboardInput.Enter },
            { Keys.Up, KeyboardInput.Up },
            { Keys.Right, KeyboardInput.Right },
            { Keys.Down, KeyboardInput.Down },
            { Keys.Left, KeyboardInput.Left },
            { Keys.D1, KeyboardInput.D1 },
            { Keys.D3, KeyboardInput.D3 },
            { Keys.Q, KeyboardInput.Q },
            { Keys.E, KeyboardInput.E },
            { Keys.W, KeyboardInput.W },
            { Keys.D, KeyboardInput.D },
            { Keys.S, KeyboardInput.S },
            { Keys.A, KeyboardInput.A }
        };

        BufferedWaveProvider.DiscardOnBufferOverflow = true;
        BufferedWaveProvider.BufferDuration          = new TimeSpan(0, 0, 0, 0, 300);
        WaveOutEvent.Init(BufferedWaveProvider);

        var diskFilename = GetDiskFilename();
        Psx = new Emulator(this, diskFilename);

        var timer = new Timer(1000);
        timer.Elapsed += OnTimedEvent;
        timer.Enabled =  true;

        RunUncapped();
    }

    public void Play(byte[] samples)
    {
        BufferedWaveProvider.AddSamples(samples, 0, samples.Length);

        if (WaveOutEvent.PlaybackState != PlaybackState.Playing)
        {
            WaveOutEvent.Play();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render(int[] buffer24, ushort[] buffer16)
    {
        //Console.WriteLine($"x1 {displayX1} x2 {displayX2} y1 {displayY1} y2 {displayY2}");

        var horizontalEnd = HorizontalRes;
        var verticalEnd = VerticalRes;

        if (IsVramViewer)
        {
            horizontalEnd = 1024;
            verticalEnd   = 512;

            Marshal.Copy(buffer24, 0, Display.BitmapData, 0x80000);
        }
        else if (Is24BitDepth)
        {
            Blit24Bpp(buffer24);
        }
        else
        {
            Blit16Bpp(buffer24);
        }

        Fps++;
        BeginInvoke(() =>
        {
            using var deviceContext = new GdiDeviceContext(Screen.Handle);

            NativeMethods.StretchBlt(deviceContext, 0, 0, Screen.Width, Screen.Height,
                Display.DeviceContext, 0, 0, horizontalEnd, verticalEnd,
                RasterOp.SRCCOPY);
        });
    }

    public void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth)
    {
        Is24BitDepth = is24BitDepth;

        if (horizontalRes != HorizontalRes || verticalRes != VerticalRes)
        {
            HorizontalRes = horizontalRes;
            VerticalRes   = verticalRes;

            ClearDisplay();
            //Console.WriteLine($"setDisplayMode {horizontalRes} {verticalRes} {is24BitDepth}");
        }
    }

    public void SetHorizontalRange(ushort displayX1, ushort displayX2)
    {
        DisplayX1 = displayX1;
        DisplayX2 = displayX2;

        //Console.WriteLine($"Horizontal Range {displayX1} {displayX2}");
    }

    public void SetVerticalRange(ushort displayY1, ushort displayY2)
    {
        DisplayY1 = displayY1;
        DisplayY2 = displayY2;

        //Console.WriteLine($"Vertical Range {displayY1} {displayY2}");
    }

    public void SetVRAMStart(ushort displayVramxStart, ushort displayVramyStart)
    {
        DisplayVramxStart = displayVramxStart;
        DisplayVramyStart = displayVramyStart;

        //Console.WriteLine($"Vram Start {displayVRAMXStart} {displayVRAMYStart}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Blit16Bpp(int[] vramBits)
    {
        //Console.WriteLine($"x1 {displayX1} x2 {displayX2} y1 {displayY1} y2 {displayY2}");
        //Console.WriteLine($"Display Height {display.Height}  Width {display.Width}");
        var yRangeOffset = (240 - (DisplayY2 - DisplayY1)) >> (VerticalRes == 480 ? 0 : 1);
        if (yRangeOffset < 0) yRangeOffset = 0;

        var vram = new Span<int>(vramBits);
        var display = new Span<int>(Display.BitmapData.ToPointer(), 0x80000);

        for (var y = yRangeOffset; y < VerticalRes - yRangeOffset; y++)
        {
            var from = vram.Slice(DisplayVramxStart + (y - yRangeOffset + DisplayVramyStart) * 1024, HorizontalRes);
            var to = display.Slice(y * 1024);
            from.CopyTo(to);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Blit24Bpp(int[] vramBits)
    {
        var yRangeOffset = (240 - (DisplayY2 - DisplayY1)) >> (VerticalRes == 480 ? 0 : 1);
        if (yRangeOffset < 0) yRangeOffset = 0;

        var display = new Span<int>(Display.BitmapData.ToPointer(), 0x80000);
        Span<int> scanLine = stackalloc int[HorizontalRes];

        for (var y = yRangeOffset; y < VerticalRes - yRangeOffset; y++)
        {
            var offset = 0;
            var startXyPosition = DisplayVramxStart + (y - yRangeOffset + DisplayVramyStart) * 1024;
            for (var x = 0; x < HorizontalRes; x += 2)
            {
                var p0Rgb = vramBits[startXyPosition + offset++];
                var p1Rgb = vramBits[startXyPosition + offset++];
                var p2Rgb = vramBits[startXyPosition + offset++];

                var p0Bgr555 = GetPixelBgr555(p0Rgb);
                var p1Bgr555 = GetPixelBgr555(p1Rgb);
                var p2Bgr555 = GetPixelBgr555(p2Rgb);

                //[(G0R0][R1)(B0][B1G1)]
                //   RG    B - R   GB

                var p0R = p0Bgr555 & 0xFF;
                var p0G = (p0Bgr555 >> 8) & 0xFF;
                var p0B = p1Bgr555 & 0xFF;
                var p1R = (p1Bgr555 >> 8) & 0xFF;
                var p1G = p2Bgr555 & 0xFF;
                var p1B = (p2Bgr555 >> 8) & 0xFF;

                var p0Rgb24Bpp = (p0R << 16) | (p0G << 8) | p0B;
                var p1Rgb24Bpp = (p1R << 16) | (p1G << 8) | p1B;

                scanLine[x]     = p0Rgb24Bpp;
                scanLine[x + 1] = p1Rgb24Bpp;
            }

            scanLine.CopyTo(display.Slice(y * 1024));
        }
    }

    private unsafe void ClearDisplay()
    {
        var span = new Span<uint>(Display.BitmapData.ToPointer(), 0x80000);
        span.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort GetPixelBgr555(int color)
    {
        var m = (byte)((color & 0xFF000000) >> 24);
        var r = (byte)((color & 0x00FF0000) >> (16 + 3));
        var g = (byte)((color & 0x0000FF00) >> (8 + 3));
        var b = (byte)((color & 0x000000FF) >> 3);

        return (ushort)((m << 15) | (b << 10) | (g << 5) | r);
    }

    private string GetDiskFilename()
    {
        var cla = Environment.GetCommandLineArgs();
        if (cla.Any(s => s.EndsWith(".bin") || s.EndsWith(".cue") || s.EndsWith(".exe")))
        {
            var filename = cla.First(s => s.EndsWith(".bin") || s.EndsWith(".cue") || s.EndsWith(".exe"));
            return filename;
        }

        //Show the user a dialog so they can pick the bin they want to load.
        var fileDialog = new OpenFileDialog();
        fileDialog.Filter = "BIN/CUE files or PSXEXEs(*.bin, *.cue, *.exe)|*.bin;*.cue;*.exe";
        fileDialog.ShowDialog();

        var file = fileDialog.FileName;
        return file;
    }

    private KeyboardInput? GetGamepadButton(Keys keyCode)
    {
        if (_gamepadKeyMap.TryGetValue(keyCode, out var gamepadButtonValue))
            return gamepadButtonValue;

        return null;
    }

    private void HandleJoyPadDown(object sender, KeyEventArgs e)
    {
        var button = GetGamepadButton(e.KeyCode);
        if (button != null)
            Psx.JoyPadDown(button.Value);
    }

    private void HandleJoyPadUp(object sender, KeyEventArgs e)
    {
        var button = GetGamepadButton(e.KeyCode);
        if (button != null)
            Psx.JoyPadUp(button.Value);
    }

    private void ToggleDebug(object sender, MouseEventArgs e)
    {
        Psx.ToggleDebug();
    }

    public int GetVps()
    {
        var currentFps = Fps;
        Fps = 0;
        return currentFps;
    }

    private void VRAMViewerToggle(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Tab)
        {
            if (!IsVramViewer)
            {
                Screen.Size = VRAMSize;
            }
            else
            {
                Screen.Size = _640x480;
            }

            IsVramViewer = !IsVramViewer;
            ClearDisplay();
        }
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() =>
            {
                Text             = $"ProjectPSX | Cpu {(int)((float)CpuCyclesCounter / (PsxMhz / MipsUnderclock) * SyncCycles)}% | Vps {GetVps()}";
                CpuCyclesCounter = 0;
            });
        }
    }

    public void RunUncapped()
    {
        var t = Task.Factory.StartNew(Execute, TaskCreationOptions.LongRunning);
    }

    private void Execute()
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        Thread.CurrentThread.Priority             = ThreadPriority.Highest;

        try
        {
            while (true)
            {
                Psx.RunFrame();
                CpuCyclesCounter += Cycles;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
}