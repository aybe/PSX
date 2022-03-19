namespace PSX.Core.Graphics.Internal;

public sealed class VRAM16 : VRAM<ushort> // TODO this should be signed just like other VRAM is
{
    public VRAM16(int width, int height) : base(width, height)
    {
    }

    public override ushort GetPixel(int x, int y)
    {
        return Pixels[GetIndex(x, y)];
    }

    public override void SetPixel(int x, int y, ushort color)
    {
        Pixels[GetIndex(x, y)] = color;
    }
}