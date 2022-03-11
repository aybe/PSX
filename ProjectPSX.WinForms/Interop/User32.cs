using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ProjectPSX.WinForms.Interop;

[SuppressUnmanagedCodeSecurity]
internal static partial class NativeMethods
{
    [DllImport(ExternDll.User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin,
        uint messageFilterMax, uint flags);

    [DllImport(ExternDll.User32)]
    internal static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport(ExternDll.User32)]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
}