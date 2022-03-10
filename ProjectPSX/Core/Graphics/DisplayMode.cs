namespace ProjectPSX.Core.Graphics;

public readonly struct DisplayMode
    // TODO this doesn't exactly mimic GP1(08h) - Display mode
{
    public DisplayMode(uint value)
    {
        HorizontalResolution1   = (byte)(value & 0x3);
        IsVerticalResolution480 = (value & 0x4) != 0;
        IsPAL                   = (value & 0x8) != 0;
        Is24BitDepth            = (value & 0x10) != 0;
        IsVerticalInterlace     = (value & 0x20) != 0;
        HorizontalResolution2   = (byte)((value & 0x40) >> 6);
        IsReverseFlag           = (value & 0x80) != 0;
    }

    public byte HorizontalResolution1 { get; }

    public bool IsVerticalResolution480 { get; }

    public bool IsPAL { get; }

    public bool Is24BitDepth { get; }

    public bool IsVerticalInterlace { get; }

    public byte HorizontalResolution2 { get; }

    public bool IsReverseFlag { get; }
}