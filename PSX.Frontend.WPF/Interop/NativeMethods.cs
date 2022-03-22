using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PSX.Frontend.WPF.Interop;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
internal static partial class NativeMethods
{
    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/allocconsole
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AllocConsole();

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
    /// </summary>
    [DllImport("user32.dll")]
    public static extern int CallNextHookEx(
        [In] [Optional] IntPtr hhk,
        [In] int nCode,
        [In] uint wParam,
        [In] ref KBDLLHOOKSTRUCT lParam
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/freeconsole
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeConsole();

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/getconsolewindow
    /// </summary>
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmenu
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetSystemMenu(
        [In] IntPtr hWnd,
        [MarshalAs(UnmanagedType.Bool)] bool bRevert
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-deletemenu
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteMenu(
        [In] IntPtr hMenu,
        uint uPosition,
        uint uFlags
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/getstdhandle
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(
        uint nStdHandle
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetConsoleCtrlHandler(
        PHANDLER_ROUTINE? HandlerRoutine,
        [MarshalAs(UnmanagedType.Bool)] bool Add
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/console/setconsoletitle
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetConsoleTitle(
        [In] [MarshalAs(UnmanagedType.LPWStr)] string lpConsoleTitle
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hmod,
        uint dwThreadId
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        [In] IntPtr hWnd,
        [In] IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwindowshookex
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(
        [In] IntPtr hhk
    );
}