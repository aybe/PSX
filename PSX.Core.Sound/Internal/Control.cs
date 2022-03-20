namespace PSX.Core.Sound.Internal;

internal struct Control
{
    public ushort Register;

    public bool SPUEnabled => ((Register >> 15) & 0x1) != 0;

    public bool SPUUnmuted => ((Register >> 14) & 0x1) != 0;

    public int NoiseFrequencyShift => (Register >> 10) & 0xF;

    public int NoiseFrequencyStep => (Register >> 8) & 0x3;

    public bool ReverbMasterEnabled => ((Register >> 7) & 0x1) != 0;

    public bool Irq9Enabled => ((Register >> 6) & 0x1) != 0;

    public int SoundRamTransferMode => (Register >> 4) & 0x3;

    public bool ExternalAudioReverb => ((Register >> 3) & 0x1) != 0;

    public bool CdAudioReverb => ((Register >> 2) & 0x1) != 0;

    public bool ExternalAudioEnabled => ((Register >> 1) & 0x1) != 0;

    public bool CdAudioEnabled => (Register & 0x1) != 0;
}