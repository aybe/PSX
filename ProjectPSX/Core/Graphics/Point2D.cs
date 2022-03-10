using System.Runtime.InteropServices;

namespace ProjectPSX.Core.Graphics;

[StructLayout(LayoutKind.Explicit)]
internal struct Point2D
{
    [FieldOffset(0)]
    public short X;

    [FieldOffset(2)]
    public short Y;
}