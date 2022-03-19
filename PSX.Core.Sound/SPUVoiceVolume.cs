namespace PSX.Core.Sound;

public struct SPUVoiceVolume
{
    public ushort Register;

    public bool IsSweepMode => ((Register >> 15) & 0x1) != 0;

    public short FixedVolume => (short)(Register << 1);

    public bool IsSweepExponential => ((Register >> 14) & 0x1) != 0;

    public bool IsSweepDirectionDecrease => ((Register >> 13) & 0x1) != 0;

    public bool IsSweepPhaseNegative => ((Register >> 12) & 0x1) != 0;

    public int SweepShift => (Register >> 2) & 0x1F;

    public int SweepStep => Register & 0x3;
}