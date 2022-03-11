namespace ProjectPSX.Core;

internal readonly struct DrawingOffset
{
    public short X { get; }

    public short Y { get; }

    public DrawingOffset(uint value)
    {
        X = GPU.Read11BitShort(value & 0x7FF);
        Y = GPU.Read11BitShort((value >> 11) & 0x7FF);
    }
}