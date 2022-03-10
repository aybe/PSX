using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ProjectPSX.Core.Graphics;

[StructLayout(LayoutKind.Explicit)]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
internal struct Color
{
    [FieldOffset(0)]
    public uint Value;

    [FieldOffset(0)]
    public byte R;

    [FieldOffset(1)]
    public byte G;

    [FieldOffset(2)]
    public byte B;

    [FieldOffset(3)]
    public byte M;
}