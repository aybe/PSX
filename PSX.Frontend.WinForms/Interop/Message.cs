using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PSX.Frontend.WinForms.Interop;

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