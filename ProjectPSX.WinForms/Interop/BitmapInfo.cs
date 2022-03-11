using System.Runtime.InteropServices;

namespace ProjectPSX.WinForms.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct BitmapInfo
{
    public BitmapInfoHeader bmiHeader;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public uint[] bmiColors;
}