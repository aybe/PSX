namespace PSX.Core.Graphics.Internal;

/// <summary>
///     GP0(E2h) - Texture Window setting
/// </summary>
public readonly struct TextureWindow
{
    public TextureWindow(uint value)
    {
        MaskX   = (byte)(value & 0x1F);
        MaskY   = (byte)((value >> 5) & 0x1F);
        OffsetX = (byte)((value >> 10) & 0x1F);
        OffsetY = (byte)((value >> 15) & 0x1F);
    }

    public byte MaskX { get; }

    public byte MaskY { get; }

    public byte OffsetX { get; }

    public byte OffsetY { get; }
}