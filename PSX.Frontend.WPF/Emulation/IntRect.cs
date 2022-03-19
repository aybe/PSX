namespace PSX.Frontend.WPF.Emulation;

public readonly struct IntRect
{
    public int X1 { get; }

    public int Y1 { get; }

    public int X2 { get; }

    public int Y2 { get; }

    public IntRect(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public override string ToString()
    {
        return $"{nameof(X1)}: {X1}, {nameof(Y1)}: {Y1}, {nameof(X2)}: {X2}, {nameof(Y2)}: {Y2}";
    }
}