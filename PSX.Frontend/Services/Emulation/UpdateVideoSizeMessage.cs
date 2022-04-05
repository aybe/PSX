namespace PSX.Frontend.Services.Emulation;

public sealed class UpdateVideoSizeMessage
{
    public UpdateVideoSizeMessage(int width, int height, UpdateVideoFormat format)
    {
        Width  = width;
        Height = height;
        Format = format;
    }

    public int Width { get; }

    public int Height { get; }

    public UpdateVideoFormat Format { get; }

    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(Format)}: {Format}";
    }
}