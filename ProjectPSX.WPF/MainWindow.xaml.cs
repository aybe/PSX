using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ProjectPSX.WPF.Emulation;
using ProjectPSX.WPF.Interop;
using ProjectPSX.WPF.Sound;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace ProjectPSX.WPF;

internal sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public string? EmulatorExecutable { get; set; }

    private WriteableBitmap? EmulatorBitmap { get; set; }

    private bool EmulatorBitmapIs24Bit { get; set; }

    private Emulator? Emulator { get; set; }

    private int EmulatorSoundStream { get; set; }

    private CancellationTokenSource? EmulatorTokenSource { get; set; }

    #region Window events

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeConsole();
        InitializeSound();
        InitializeEmulator();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        CleanupEmulator();
        CleanupSound();
        CleanupConsole();
    }

    #endregion

    #region Initialization/cleanup

    private static void InitializeConsole()
    {
        NativeMethods.AllocConsole();
    }

    private static void CleanupConsole()
    {
        NativeMethods.FreeConsole();
    }

    private void InitializeSound()
    {
        var helper = new WindowInteropHelper(this);
        var handle = helper.Handle;

        if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle))
            throw new BassException("Couldn't initialize BASS.");

        var mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_MIXER_NONSTOP);
        if (mixer is 0)
            throw new BassException("Couldn't create mixer stream.");

        var push = Bass.BASS_StreamCreatePush(44100, 2, BASSFlag.BASS_STREAM_DECODE, IntPtr.Zero);
        if (push is 0)
            throw new BassException("Couldn't create push stream.");

        if (!BassMix.BASS_Mixer_StreamAddChannel(mixer, push, BASSFlag.BASS_DEFAULT))
            throw new BassException("Couldn't add push channel to mixer stream.");

        if (!Bass.BASS_ChannelPlay(mixer, false))
            throw new BassException("Couldn't play mixer channel.");

        EmulatorSoundStream = push;
    }

    private void CleanupSound()
    {
        if (!Bass.BASS_Free())
            throw new BassException("Couldn't free BASS.");

        EmulatorSoundStream = 0;
    }

    private void InitializeEmulator()
    {
        if (EmulatorExecutable is null)
            return;

        var window = new HostWindow
        {
            UpdateBitmapSize = UpdateBitmapSize,
            UpdateBitmapData = UpdateBitmapData,
            UpdateSampleData = UpdateSampleData
        };

        Emulator = new Emulator(window, EmulatorExecutable);

        EmulatorTokenSource = new CancellationTokenSource();

        var token = EmulatorTokenSource.Token;

        Task.Factory.StartNew(() => Update(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    private void CleanupEmulator()
    {
        EmulatorTokenSource?.Cancel();

        Emulator?.Dispose();
    }

    #endregion

    #region Update callbacks

    private void Update(CancellationToken token)
    {
        try
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                Emulator?.RunFrame();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void UpdateBitmapData(IntSize size, IntRect rect, int[] buffer24, ushort[] buffer16)
    {
        Dispatcher.BeginInvoke(UpdateBitmapDataProc, size, rect, buffer24, buffer16);
    }

    private unsafe void UpdateBitmapDataProc(IntSize size, IntRect rect, int[] buffer24, ushort[] buffer16)
    {
        using var context = EmulatorBitmap.GetBitmapContext(ReadWriteMode.ReadWrite);

        if (EmulatorBitmapIs24Bit)
        {
            var span = MemoryMarshal.Cast<ushort, byte>(buffer16);

            for (var y = 0; y < context.Height; y++)
            {
                for (var x = 0; x < context.Width; x++)
                {
                    var i = y * 2048 + x * 3;
                    var r = span[i + 0];
                    var g = span[i + 1];
                    var b = span[i + 2];
                    var j = context.Pixels + (y * context.Width + x);
                    *j = (255 << 24) | (r << 16) | (g << 8) | b;
                }
            }
        }
        else
        {
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

    private void UpdateBitmapSize(IntSize size, bool is24Bit)
    {
        Dispatcher.BeginInvoke(UpdateBitmapSizeProc, size, is24Bit);
    }

    private void UpdateBitmapSizeProc(IntSize size, bool is24Bit)
    {
        EmulatorBitmap = BitmapFactory.New(size.X, size.Y);

        EmulatorBitmapIs24Bit = is24Bit;

        Image1.Source = EmulatorBitmap;

        Title = $"Width = {size.X}, Height = {size.Y}, 24-bit = {is24Bit}";
    }

    private void UpdateSampleData(byte[] buffer)
    {
        if (EmulatorSoundStream is 0)
            return;

        var data = Bass.BASS_StreamPutData(EmulatorSoundStream, buffer, buffer.Length);

        if (data is not -1)
            return;

        var exception = new BassException("Couldn't put data in push stream.");

        if (exception.Error is not BASSError.BASS_ERROR_HANDLE)
        {
            throw exception;
        }
    }

    #endregion
}