using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace ProjectPSX.WPF;

public partial class MainWindow
{
    private Emulator _emulator = null!;

    private int _emulatorCycles;

    public MainWindow()
    {
        InitializeComponent();
    }

    public string? Executable { get; set; }

    private WriteableBitmap Bitmap { get; set; } // BUG

    private bool Bitmap24 { get; set; }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        NativeMethods.AllocConsole();
        var helper = new WindowInteropHelper(this);
        var handle = helper.Handle;

        if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle))
        {
            throw new BassException();
        }

        var mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_MIXER_NONSTOP);

        if (mixer is 0)
        {
            throw new BassException();
        }

        var push = Bass.BASS_StreamCreatePush(44100, 2, BASSFlag.BASS_STREAM_DECODE, IntPtr.Zero);

        if (push == 0)
        {
            throw new BassException();
        }

        if (!BassMix.BASS_Mixer_StreamAddChannel(mixer, push, BASSFlag.BASS_DEFAULT))
        {
            throw new BassException();
        }

        if (!Bass.BASS_ChannelPlay(mixer, false))
        {
            throw new BassException();
        }

        var window = new WpfHostWindow();

        window.UpdateBitmapSize = UpdateBitmapSize;
        window.UpdateBitmapData = UpdateBitmapData;
        window.StreamPush = push;
        window.StreamMixer = mixer;

        _emulator = new Emulator(window, Executable);

        Task.Factory.StartNew(UpdateLoop, TaskCreationOptions.LongRunning);
    }

    private void UpdateBitmapData(IntSize size, IntRect rect, int[] buffer24, ushort[] buffer16)
    {
        Dispatcher.BeginInvoke(UpdateBitmapDataProc, size, rect, buffer24, buffer16);
    }

    private unsafe void UpdateBitmapDataProc(IntSize size, IntRect rect, int[] buffer24, ushort[] buffer16)
    {
        // var b24 = MemoryMarshal.AsBytes<int>(buffer24).ToArray();
        // var b16 = MemoryMarshal.AsBytes<ushort>(buffer16).ToArray();
        var cast = MemoryMarshal.Cast<ushort, byte>(buffer16);

        // File.WriteAllBytes(@"C:\temp\vr24.bin", b24);
        // File.WriteAllBytes(@"C:\temp\vr16.bin", b16);

        using (var context = Bitmap.GetBitmapContext(ReadWriteMode.ReadWrite))
        {
            if (Bitmap24)
            {
                for (var y = 0; y < context.Height; y++)
                {
                    for (var x = 0; x < context.Width; x++)
                    {
                        var i = (/*size.Y +*/ y) * 2048 + (/*size.X +*/ x) * 3;
                        var r = cast[i + 0];
                        var g = cast[i + 1];
                        var b = cast[i + 2];
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            else
            {
                //for (var y = 0; y < context.Height; y++)
                //{
                //    break;
                //    for (var x = 0; x < context.Width; x++)
                //    {
                //        var src = (size.Y + y) * 1024 + size.X + x;
                //        var dst = y * context.Width + x;
                //        var ptr = context.Pixels + dst;
                //        var pxl = buffer24[src];
                //        *ptr = pxl | unchecked((int)0xFF000000);
                //    }
                //}
                //context.Clear();

                for (var y = 0; y < context.Height; y++)
                {
                    for (var x = 0; x < context.Width; x++)
                    {
                        var i = (size.Y + y) * 1024 + (size.X + x) * 1;
                        var r = ((buffer16[i] >> 00) & 0b11111) * 255 / 31;
                        var g = ((buffer16[i] >> 05) & 0b11111) * 255 / 31;
                        var b = ((buffer16[i] >> 10) & 0b11111) * 255 / 31;
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
        }
    }

    private void UpdateBitmapSize(IntSize size, bool bpp24)
    {
        Bitmap24 = bpp24; // TODO

        Dispatcher.BeginInvoke(() =>
        {
            Bitmap = BitmapFactory.New(size.X, size.Y);
            Image1.Source = Bitmap;
            Title = $"Width = {size.X}, Height = {size.Y}, 24-bit = {bpp24}";
        });
    }

    private void UpdateLoop()
    {
        try
        {
            while (true)
            {
                _emulator.RunFrame();
                _emulatorCycles++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // MessageBox.Show(e.Message);
        }
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        _emulator.Dispose();

        if (!Bass.BASS_Free())
        {
            throw new BassException();
        }

        NativeMethods.FreeConsole();
    }
}