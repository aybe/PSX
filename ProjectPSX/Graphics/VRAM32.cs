using System;
using System.Runtime.CompilerServices;

namespace ProjectPSX.Graphics;

public sealed class VRAM32 : VRAM<int>
{
    public VRAM32(int width, int height) : base(width, height)
    {
    }

    [Obsolete("This hack shouldn't exist.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort GetPixelBgr555(int x, int y)
    {
        var color = Pixels[x + y * Width];

        var s = (byte)((color & 0xFF000000) >> 24);
        var r = (byte)((color & 0x00FF0000) >> (16 + 3));
        var g = (byte)((color & 0x0000FF00) >> (8 + 3));
        var b = (byte)((color & 0x000000FF) >> 3);

        return (ushort)((s << 15) | (b << 10) | (g << 5) | r);
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