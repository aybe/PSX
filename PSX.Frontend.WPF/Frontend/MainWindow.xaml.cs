using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.WPF.Frontend.Messages;
using PSX.Frontend.WPF.Sound;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace PSX.Frontend.WPF.Frontend;

internal sealed partial class MainWindow :
    IRecipient<UpdateAudioDataMessage>,
    IRecipient<UpdateVideoDataMessage>,
    IRecipient<UpdateVideoSizeMessage>
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = Model = App.Current.Services.GetService<MainModel>() ?? throw new InvalidOperationException();

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private MainModel Model { get; }

    private WriteableBitmap? EmulatorBitmap { get; set; }

    private bool EmulatorBitmapIs24Bit { get; set; }

    private int EmulatorSoundStream { get; set; }

    void IRecipient<UpdateAudioDataMessage>.Receive(UpdateAudioDataMessage message)
    {
        if (EmulatorSoundStream is 0)
            return;

        var data = Bass.BASS_StreamPutData(EmulatorSoundStream, message.Buffer, message.Buffer.Length);

        if (data is not -1)
            return;

        var exception = new BassException("Couldn't put data in push stream.");

        if (exception.Error is not BASSError.BASS_ERROR_HANDLE)
        {
            throw exception;
        }
    }

    unsafe void IRecipient<UpdateVideoDataMessage>.Receive(UpdateVideoDataMessage message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            using var context = EmulatorBitmap.GetBitmapContext(ReadWriteMode.ReadWrite);

            if (EmulatorBitmapIs24Bit)
            {
                var span = MemoryMarshal.Cast<ushort, byte>(message.Buffer16);

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
                        var i = (message.Size.Height + y) * 1024 + (message.Size.Width + x) * 1;
                        var r = ((message.Buffer16[i] >> 00) & 0b11111) * 255 / 31;
                        var g = ((message.Buffer16[i] >> 05) & 0b11111) * 255 / 31;
                        var b = ((message.Buffer16[i] >> 10) & 0b11111) * 255 / 31;
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
        });
    }

    void IRecipient<UpdateVideoSizeMessage>.Receive(UpdateVideoSizeMessage message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            EmulatorBitmap = BitmapFactory.New(message.Size.Width, message.Size.Height);

            EmulatorBitmapIs24Bit = message.Is24Bit;

            Image1.Source = EmulatorBitmap;

            Title = $"Width = {message.Size.Width}, Height = {message.Size.Height}, 24-bit = {message.Is24Bit}";
        });
    }

    #region Window events

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Model.IsActive = true;

        InitializeConsole();
        InitializeSound();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Model.IsActive = false;

        CleanupSound();
        CleanupConsole();
    }

    #endregion

    #region Initialization/cleanup

    private static void InitializeConsole()
    {
        //NativeMethods.AllocConsole();
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

    private static void CleanupConsole()
    {
        // NativeMethods.FreeConsole();
    }

    private void CleanupSound()
    {
        if (!Bass.BASS_Free())
            throw new BassException("Couldn't free BASS.");

        EmulatorSoundStream = 0;
    }

    #endregion
}