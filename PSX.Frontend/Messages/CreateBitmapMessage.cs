namespace PSX.Frontend.Messages;

public sealed class CreateBitmapMessage
{
    public CreateBitmapMessage(int width, int height, CreateBitmapFormat format)
    {
        Width  = width;
        Height = height;
        Format = format;
    }

    public int Width { get; }

    public int Height { get; }

    public CreateBitmapFormat Format { get; }

    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(Format)}: {Format}";
    }
}