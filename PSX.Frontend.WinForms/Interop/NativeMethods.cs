using System;
using System.Runtime.InteropServices;
using System.Security;

namespace PSX.Frontend.WinForms.Interop;

[SuppressUnmanagedCodeSecurity]
internal static partial class NativeMethods
{
    [DllImport(ExternDll.Gdi32)]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport(ExternDll.Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteDC(IntPtr hdc);

    [DllImport(ExternDll.Gdi32)]
    internal static extern IntPtr CreateDIBSection(IntPtr hdc, [In] in BitmapInfo pbmi, ColorUsage usage,
        out IntPtr ppvBits, IntPtr hSection, uint offset);

    [DllImport(ExternDll.Gdi32)]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport(ExternDll.Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject(IntPtr ho);

    [DllImport(ExternDll.Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc,
        RasterOp rop);

    [DllImport(ExternDll.Gdi32)]
    internal static extern int SetStretchBltMode(IntPtr hdc, BltMode mode);

    [DllImport(ExternDll.Gdi32)]
    internal static extern int StretchDIBits(IntPtr hdc, int xDest, int yDest, int DestWidth, int destHeight,
        int xSrc, int ySrc, int srcWidth, int srcHeight, IntPtr lpBits,
        [In] ref BitmapInfo lpbmi, ColorUsage iUsage, RasterOp rop);
}