namespace ProjectPSX.Sound;

internal struct SPUControl
{
    public ushort register;

    public bool spuEnabled => ((register >> 15) & 0x1) != 0;

    public bool spuUnmuted => ((register >> 14) & 0x1) != 0;

    public int noiseFrequencyShift => (register >> 10) & 0xF;

    public int noiseFrequencyStep => (register >> 8) & 0x3;

    public bool reverbMasterEnabled => ((register >> 7) & 0x1) != 0;

    public bool irq9Enabled => ((register >> 6) & 0x1) != 0;

    public int soundRamTransferMode => (register >> 4) & 0x3;

    public bool externalAudioReverb => ((register >> 3) & 0x1) != 0;

    public bool cdAudioReverb => ((register >> 2) & 0x1) != 0;

    public bool externalAudioEnabled => ((register >> 1) & 0x1) != 0;

    public bool cdAudioEnabled => (register & 0x1) != 0;
}