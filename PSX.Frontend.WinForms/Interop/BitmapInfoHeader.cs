using System.Runtime.InteropServices;

namespace PSX.Frontend.WinForms.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct BitmapInfoHeader
{
    public uint              biSize;
    public int               biWidth;
    public int               biHeight;
    public ushort            biPlanes;
    public ushort            biBitCount;
    public BitmapCompression biCompression;
    public uint              biSizeImage;
    public int               biXPelsPerMeter;
    public int               biYPelsPerMeter;
    public uint              biClrUsed;
    public uint              biClrImportant;
}