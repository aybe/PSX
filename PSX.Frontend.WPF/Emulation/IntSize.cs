namespace PSX.Frontend.WPF.Emulation;

public readonly struct IntSize
{
    public int Width { get; }

    public int Height { get; }

    public IntSize(int width, int height)
    {
        Width  = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}";
    }
}