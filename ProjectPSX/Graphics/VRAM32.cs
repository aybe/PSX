namespace ProjectPSX.Graphics;

public sealed class VRAM32 : VRAM<int>
{
    public VRAM32(int width, int height) : base(width, height)
    {
    }

    public override int GetPixel(int x, int y)
    {
        return Pixels[GetIndex(x, y)];
    }

    public override void SetPixel(int x, int y, int color)
    {
        Pixels[GetIndex(x, y)] = color;
    }
}