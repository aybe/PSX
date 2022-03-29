using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Core.Modules;
using PSX.Frontend.Core.Navigation;
using PSX.Frontend.WPF.Interop;
using PSX.Logging.Obsolete;
using Serilog.Events;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace PSX.Frontend.WPF.Views;

internal sealed partial class ViewShell :
    IViewShell,
    INavigationTarget,
    IRecipient<EmulatorUpdateAudioDataMessage>,
    IRecipient<EmulatorUpdateVideoDataMessage>,
    IRecipient<EmulatorUpdateVideoSizeMessage>
{
    public ViewShell(ViewModelShell model)
    {
        InitializeComponent();

        DataContext = Model = model;

        ConsoleKeyboardHookProcRef = ConsoleKeyboardHookProc; // prevent garbage collection

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private ViewModelShell Model { get; }

    private WriteableBitmap? EmulatorBitmap { get; set; }

    private bool EmulatorBitmapIs24Bit { get; set; }

    private int EmulatorSoundStream { get; set; }

    void IRecipient<EmulatorUpdateAudioDataMessage>.Receive(EmulatorUpdateAudioDataMessage message)
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

    unsafe void IRecipient<EmulatorUpdateVideoDataMessage>.Receive(EmulatorUpdateVideoDataMessage message)
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

    void IRecipient<EmulatorUpdateVideoSizeMessage>.Receive(EmulatorUpdateVideoSizeMessage message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            EmulatorBitmap = BitmapFactory.New(message.Size.Width, message.Size.Height);

            EmulatorBitmapIs24Bit = message.Is24Bit;

            Image1.Source = EmulatorBitmap;

            Title = $"Width = {message.Size.Width}, Height = {message.Size.Height}, 24-bit = {message.Is24Bit}";
        });
    }

    private sealed class BassException : Exception
    {
        public BassException(string? message = null)
        {
            Error = Bass.BASS_ErrorGetCode();

            Message = message is null ? string.Empty : $"{message}: {Error}";
        }

        public BASSError Error { get; }

        public override string Message { get; }
    }

    #region Window events

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Model.IsActive = true;

        // InitializeConsole();
        InitializeLogging();
        InitializeSound();

        Activate(); // don't let console steal focus
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Model.IsActive = false;

        CleanupSound();
        // CleanupConsole();
    }

    #endregion

    #region Console stuff

    private IntPtr ConsoleHandle;

    private IntPtr ConsoleKeyboardHookProcHandle;

    private readonly LowLevelKeyboardProc ConsoleKeyboardHookProcRef;

    [SuppressMessage("ReSharper", "InvertIf")]
    private int ConsoleKeyboardHookProc(int nCode, uint wParam, ref KBDLLHOOKSTRUCT lParam)
        // prevent Alt-F4 on console
    {
        if (nCode is NativeConstants.HC_ACTION && NativeMethods.GetForegroundWindow() == ConsoleHandle)
        {
            switch (wParam)
            {
                case NativeConstants.WM_KEYDOWN:
                case NativeConstants.WM_KEYUP:
                case NativeConstants.WM_SYSKEYDOWN:
                case NativeConstants.WM_SYSKEYUP:

                    if (lParam.vkCode == NativeConstants.VK_F4 && lParam.flags == NativeConstants.LLKHF_ALTDOWN)
                    {
                        return 1;
                    }

                    break;
            }
        }

        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, ref lParam);
    }

    #endregion

    #region Initialization/cleanup

    private static void InitializeLogging()
    {
        LoggingFactory.Initialize();

        LoggingFactory.LevelSwitch.MinimumLevel = LogEventLevel.Verbose;
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private void InitializeConsole()
    {
        // allocate a console and set its title

        if (!NativeMethods.AllocConsole())
            throw new Win32Exception();

        if (!NativeMethods.SetConsoleTitle("PSX Debug Console"))
            throw new Win32Exception();

        // set console position to top/left

        // https://stackoverflow.com/questions/42905649/cant-center-my-console-window-by-using-the-following-code

        var handle = NativeMethods.GetStdHandle(NativeConstants.STD_OUTPUT_HANDLE);
        if (handle is NativeConstants.INVALID_HANDLE_VALUE)
            throw new Win32Exception();

        ConsoleHandle = NativeMethods.GetConsoleWindow();

        if (ConsoleHandle == IntPtr.Zero)
            throw new InvalidOperationException();

        var after = new WindowInteropHelper(this).Handle;

        if (!NativeMethods.SetWindowPos(ConsoleHandle, after, 0, 0, 0, 0, NativeConstants.SWP_NOSIZE))
            throw new Win32Exception();

        // remove the close button

        // https://stackoverflow.com/questions/11959643/why-does-closing-a-console-that-was-started-with-allocconsole-cause-my-whole-app

        if (!NativeMethods.SetConsoleCtrlHandler(null, true))
            throw new Win32Exception();

        var systemMenu = NativeMethods.GetSystemMenu(ConsoleHandle, false);
        if (systemMenu == IntPtr.Zero)
            throw new InvalidOperationException();

        if (!NativeMethods.DeleteMenu(systemMenu, NativeConstants.SC_CLOSE, NativeConstants.MF_BYCOMMAND))
            throw new Win32Exception();

        // setup a hook to intercept and prevent Alt-F4

        // https://tpiros.dev/blog/c-disable-ctrl-alt-del-alt-tab-alt-f4-start-menu-and-so-on/

        ConsoleKeyboardHookProcHandle = NativeMethods.SetWindowsHookEx(NativeConstants.WH_KEYBOARD_LL, ConsoleKeyboardHookProcRef, IntPtr.Zero, 0);

        if (ConsoleKeyboardHookProcHandle == IntPtr.Zero)
        {
            throw new Win32Exception();
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private void CleanupConsole()
    {
        if (!NativeMethods.UnhookWindowsHookEx(ConsoleKeyboardHookProcHandle))
            throw new Win32Exception();

        if (!NativeMethods.SetConsoleCtrlHandler(null, false))
            throw new Win32Exception();

        if (!NativeMethods.FreeConsole())
            throw new Win32Exception();
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

    #endregion
}