namespace PSX.Core.Graphics.Internal;

public abstract class VRAM<T> : IVRAM<T> where T : unmanaged
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

    public int Width { get; }

    public int Height { get; }

    public T[] Pixels { get; }

    public abstract T GetPixel(int x, int y);

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