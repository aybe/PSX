using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PSX.Devices.Input;

public sealed class MouseController : ControllerBase
    // TODO albeit working it still needs a lot of polish
    // BUG there are artifacts on display while moving mouse
{
    private POINT Point;

    public override ushort Type { get; } = 0x5A12;

    public override void GenerateResponse()
    {
        base.GenerateResponse();

        var swapped = NativeMethods.GetSystemMetrics(NativeConstants.SM_SWAPBUTTON) is not 0;
        var lButton = NativeMethods.GetKeyState(swapped ? NativeConstants.VK_LBUTTON : NativeConstants.VK_RBUTTON);
        var rButton = NativeMethods.GetKeyState(swapped ? NativeConstants.VK_RBUTTON : NativeConstants.VK_LBUTTON);
        var rActive = (byte)(((lButton & 0x8000) != 0 ? 0 : 1) << 2);
        var lActive = (byte)(((rButton & 0x8000) != 0 ? 0 : 1) << 3);

        var b1 = (byte)(0xF0 | rActive | lActive);

        if (!NativeMethods.GetCursorPos(out var point))
            throw new Win32Exception();

        var dx = point.x - Point.x;
        var dy = point.y - Point.y;

        Point = point;

        var b2 = (byte)Math.Clamp(dx, sbyte.MinValue, sbyte.MaxValue);
        var b3 = (byte)Math.Clamp(dy, sbyte.MinValue, sbyte.MaxValue);

        TransferDataFifo.Enqueue(0xFF);
        TransferDataFifo.Enqueue(b1);
        TransferDataFifo.Enqueue(b2);
        TransferDataFifo.Enqueue(b3);
    }

    /// <summary>
    ///     https://docs.microsoft.com/en-us/previous-versions/dd162805(v=vs.85)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private struct POINT
    {
        public readonly int x;
        public readonly int y;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private static class NativeConstants
    {
        public const int SM_SWAPBUTTON = 23;

        public const int VK_LBUTTON = 0x01;

        public const int VK_RBUTTON = 0x02;
    }

    private static class NativeMethods
    {
        /// <summary>
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos([Out] out POINT lpPoint);

        /// <summary>
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeystate
        /// </summary>
        [DllImport("user32.dll")]
        public static extern short GetKeyState([In] int vKey);

        /// <summary>
        ///     https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
        /// </summary>
        /// <param name="nIndex"></param>
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics([In] int nIndex);
    }
}