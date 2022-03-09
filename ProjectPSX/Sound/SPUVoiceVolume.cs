namespace ProjectPSX.Devices.Spu;

public struct SPUVoiceVolume
{
    public ushort register;

    public bool isSweepMode => ((register >> 15) & 0x1) != 0;

    public short fixedVolume => (short)(register << 1);

    public bool isSweepExponential => ((register >> 14) & 0x1) != 0;

    public bool isSweepDirectionDecrease => ((register >> 13) & 0x1) != 0;

    public bool isSweepPhaseNegative => ((register >> 12) & 0x1) != 0;

    public int sweepShift => (register >> 2) & 0x1F;

    public int sweepStep => register & 0x3;
}