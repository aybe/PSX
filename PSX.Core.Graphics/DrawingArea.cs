namespace PSX.Core.Graphics;

public readonly struct DrawingArea : IEquatable<DrawingArea>
{
    public ushort X { get; }

    public ushort Y { get; }

    public DrawingArea(uint value)
    {
        X = (ushort)(value & 0x3FF);
        Y = (ushort)((value >> 10) & 0x1FF);
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }

    public bool Equals(DrawingArea other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is DrawingArea other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(DrawingArea left, DrawingArea right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DrawingArea left, DrawingArea right)
    {
        return !left.Equals(right);
    }
}