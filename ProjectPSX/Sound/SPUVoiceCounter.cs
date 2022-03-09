namespace ProjectPSX.Sound;

public struct SPUVoiceCounter
{
    //internal
    public uint Register;

    public uint CurrentSampleIndex
    {
        get => (Register >> 12) & 0x1F;
        set
        {
            Register =  (ushort)(Register &= 0xFFF);
            Register |= value << 12;
        }
    }

    public uint InterpolationIndex => (Register >> 3) & 0xFF;
}