namespace PSX.Frontend.Messages;

public sealed class UpdateBitmapMessage
{
    public UpdateBitmapMessage(int startX, int startY, int xMin, int xMax, int yMin, int yMax, Array buffer16, Array buffer24)
    {
        StartX   = startX;
        StartY   = startY;
        XMin     = xMin;
        XMax     = xMax;
        YMin     = yMin;
        YMax     = yMax;
        Buffer16 = buffer16;
        Buffer24 = buffer24;
    }

    public int StartX { get; }

    public int StartY { get; }

    public int XMin { get; }

    public int XMax { get; }

    public int YMin { get; }

    public int YMax { get; }

    public Array Buffer16 { get; }

    public Array Buffer24 { get; }
}