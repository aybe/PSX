using System.Runtime.InteropServices;

namespace PSX.Core.Graphics.Internal;

[StructLayout(LayoutKind.Explicit)]
internal struct TextureData
{
    [FieldOffset(0)]
    public ushort Value;

    [FieldOffset(0)]
    public byte X;

    [FieldOffset(1)]
    public byte Y;
}