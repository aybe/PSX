using JetBrains.Annotations;

namespace PSX.Core.Graphics.Internal;

public abstract class VRAM<T> where T : unmanaged
{
    protected VRAM(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        Width  = width;
        Height = height;
        Pixels = new T[width * height];
    }

    [PublicAPI]
    public int Width { get; }

    [PublicAPI]
    public int Height { get; }

    [PublicAPI]
    public T[] Pixels { get; }

    [PublicAPI]
    public abstract T GetPixel(int x, int y);

    [PublicAPI]
    public abstract void SetPixel(int x, int y, T color);

    protected int GetIndex(int x, int y)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException(nameof(x), x, null);

        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException(nameof(y), y, null);

        var offset = y * Width + x;

        return offset;
    }
}