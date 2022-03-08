namespace ProjectPSX.WPF.Emulation;

public readonly struct IntSize
{
    public int X { get; }

    public int Y { get; }

    public IntSize(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}