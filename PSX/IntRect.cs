using System;

namespace PSX;

public readonly struct IntRect : IEquatable<IntRect>
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

    public bool Equals(IntRect other)
    {
        return XMin == other.XMin && YMin == other.YMin && XMax == other.XMax && YMax == other.YMax;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntRect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(XMin, YMin, XMax, YMax);
    }

    public static bool operator ==(IntRect left, IntRect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IntRect left, IntRect right)
    {
        return !left.Equals(right);
    }
}