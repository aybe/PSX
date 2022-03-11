using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ProjectPSX.WinForms.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct Message
{
    public IntPtr hWnd;
    public uint   msg;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint   time;
    public Point  p;
}