namespace PSX.Frontend.WPF.Emulation;

public readonly struct IntRect
{
    public int XMin { get; }

    public int YMin { get; }

    public int XMax { get; }

    public int YMax { get; }

    public IntRect(int xMin, int yMin, int xMax, int yMax)
    {
        XMin = xMin;
        YMin = yMin;
        XMax = xMax;
        YMax = yMax;
    }

    public override string ToString()
    {
        return $"{nameof(XMin)}: {XMin}, {nameof(YMin)}: {YMin}, {nameof(XMax)}: {XMax}, {nameof(YMax)}: {YMax}";
    }
}