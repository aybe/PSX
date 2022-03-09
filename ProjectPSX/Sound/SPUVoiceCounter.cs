namespace ProjectPSX.Devices.Spu;

public struct SPUVoiceCounter
{
    //internal
    public uint register;

    public uint currentSampleIndex
    {
        get => (register >> 12) & 0x1F;
        set
        {
            register =  (ushort)(register &= 0xFFF);
            register |= value << 12;
        }
    }

    public uint interpolationIndex => (register >> 3) & 0xFF;
}