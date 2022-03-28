namespace PSX.Frontend.Core.Emulation;

public readonly struct IntSize : IEquatable<IntSize>
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

    public bool Equals(IntSize other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntSize other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public static bool operator ==(IntSize left, IntSize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IntSize left, IntSize right)
    {
        return !left.Equals(right);
    }
}