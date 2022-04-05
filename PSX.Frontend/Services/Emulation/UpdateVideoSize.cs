namespace PSX.Frontend.Services.Emulation;

public sealed class UpdateVideoSize
{
    public UpdateVideoSize(int width, int height, UpdateVideoSizeFormat format)
    {
        Width  = width;
        Height = height;
        Format = format;
    }

    public int Width { get; }

    public int Height { get; }

    public UpdateVideoSizeFormat Format { get; }

    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(Format)}: {Format}";
    }
}