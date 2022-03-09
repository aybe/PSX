using System;

namespace ProjectPSX.Sound;

public class SPUVoice
{
    public ushort AdpcmRepeatAddress; //E

    public SPUVoiceAdsr Adsr;

    private int AdsrCounter;

    public SPUVoicePhase AdsrPhase;

    public ushort AdsrVolume; //C

    public SPUVoiceCounter Counter;
    public ushort CurrentAddress; //6 Internal
    public short[] DecodedSamples = new short[31]; //28 samples from current block + 3 to make room for interpolation

    public bool HasSamples;

    public short Latest;

    public short Old;
    public short Older;

    public ushort Pitch; //4

    public bool ReadRamIrq;

    public byte[] SPUAdpcm = new byte[16];
    public ushort StartAddress; //6

    public SPUVoiceVolume VolumeLeft; //0
    public SPUVoiceVolume VolumeRight; //2

    public SPUVoice()
    {
        AdsrPhase = SPUVoicePhase.Off;
    }

    private static ReadOnlySpan<sbyte> PositiveXaAdpcmTable => new sbyte[] { 0, 60, 115, 98, 122 };

    private static ReadOnlySpan<sbyte> NegativeXaAdpcmTable => new sbyte[] { 0, 0, -52, -55, -60 };

    public void keyOn()
    {
        HasSamples     = false;
        Old            = 0;
        Older          = 0;
        CurrentAddress = StartAddress;
        AdsrCounter    = 0;
        AdsrVolume     = 0;
        AdsrPhase      = SPUVoicePhase.Attack;
    }

    public void KeyOff()
    {
        AdsrCounter = 0;
        AdsrPhase   = SPUVoicePhase.Release;
    }

    internal void DecodeSamples(byte[] ram, ushort ramIrqAddress)
    {
        //save the last 3 samples from the last decoded block
        //this are needed for interpolation in case the voice.counter.currentSampleIndex is 0 1 or 2
        DecodedSamples[2] = DecodedSamples[DecodedSamples.Length - 1];
        DecodedSamples[1] = DecodedSamples[DecodedSamples.Length - 2];
        DecodedSamples[0] = DecodedSamples[DecodedSamples.Length - 3];

        Array.Copy(ram, CurrentAddress * 8, SPUAdpcm, 0, 16);

        //ramIrqAddress is >> 8 so we only need to check for currentAddress and + 1
        ReadRamIrq |= CurrentAddress == ramIrqAddress || CurrentAddress + 1 == ramIrqAddress;

        var shift = 12 - (SPUAdpcm[0] & 0x0F);
        var filter = (SPUAdpcm[0] & 0x70) >> 4; //filter on SPU adpcm is 0-4 vs XA wich is 0-3
        if (filter > 4)
            filter = 4; //Crash Bandicoot sets this to 7 at the end of the first level and overflows the filter

        int f0 = PositiveXaAdpcmTable[filter];
        int f1 = NegativeXaAdpcmTable[filter];

        //Actual ADPCM decoding is the same as on XA but the layout here is sequencial by nibble where on XA in grouped by nibble line
        var position = 2; //skip shift and flags
        var nibble = 1;
        for (var i = 0; i < 28; i++)
        {
            nibble = (nibble + 1) & 0x1;

            var t = Signed4Bit((byte)((SPUAdpcm[position] >> (nibble * 4)) & 0x0F));
            var s = (t << shift) + (Old * f0 + Older * f1 + 32) / 64;
            var sample = (short)Math.Clamp(s, -0x8000, 0x7FFF);

            DecodedSamples[3 + i] = sample;

            Older = Old;
            Old   = sample;

            position += nibble;
        }
    }

    public static int Signed4Bit(byte value)
    {
        return (value << 28) >> 28;
    }

    internal short ProcessVolume(SPUVoiceVolume volume)
    {
        if (!volume.IsSweepMode)
        {
            return volume.FixedVolume;
        }

        return 0; //todo handle sweep mode volume envelope
    }

    internal void TickAdsr(int v)
    {
        if (AdsrPhase == SPUVoicePhase.Off)
        {
            AdsrVolume = 0;
            return;
        }

        int adsrTarget;
        int adsrShift;
        int adsrStep;
        bool isDecreasing;
        bool isExponential;

        //Todo move out of tick the actual change of phase
        switch (AdsrPhase)
        {
            case SPUVoicePhase.Attack:
                adsrTarget    = 0x7FFF;
                adsrShift     = Adsr.AttackShift;
                adsrStep      = 7 - Adsr.AttackStep; // reg is 0-3 but values are "+7,+6,+5,+4"
                isDecreasing  = false; // Allways increase till 0x7FFF
                isExponential = Adsr.IsAttackModeExponential;
                break;
            case SPUVoicePhase.Decay:
                adsrTarget    = (Adsr.SustainLevel + 1) * 0x800;
                adsrShift     = Adsr.DecayShift;
                adsrStep      = -8;
                isDecreasing  = true; // Allways decreases (till target)
                isExponential = true; // Allways exponential
                break;
            case SPUVoicePhase.Sustain:
                adsrTarget    = 0;
                adsrShift     = Adsr.SustainShift;
                adsrStep      = Adsr.IsSustainDirectionDecrease ? -8 + Adsr.SustainStep : 7 - Adsr.SustainStep;
                isDecreasing  = Adsr.IsSustainDirectionDecrease; //till keyoff
                isExponential = Adsr.IsSustainModeExponential;
                break;
            case SPUVoicePhase.Release:
                adsrTarget    = 0;
                adsrShift     = Adsr.ReleaseShift;
                adsrStep      = -8;
                isDecreasing  = true; // Allways decrease till 0
                isExponential = Adsr.IsReleaseModeExponential;
                break;
            default:
                adsrTarget    = 0;
                adsrShift     = 0;
                adsrStep      = 0;
                isDecreasing  = false;
                isExponential = false;
                break;
        }

        //Envelope Operation depending on Shift/Step/Mode/Direction
        //AdsrCycles = 1 SHL Max(0, ShiftValue-11)
        //AdsrStep = StepValue SHL Max(0,11-ShiftValue)
        //IF exponential AND increase AND AdsrLevel>6000h THEN AdsrCycles=AdsrCycles*4    
        //IF exponential AND decrease THEN AdsrStep = AdsrStep * AdsrLevel / 8000h
        //Wait(AdsrCycles); cycles counted at 44.1kHz clock
        //AdsrLevel=AdsrLevel+AdsrStep  ;saturated to 0..+7FFFh

        if (AdsrCounter > 0)
        {
            AdsrCounter--;
            return;
        }

        var envelopeCycles = 1 << Math.Max(0, adsrShift - 11);
        var envelopeStep = adsrStep << Math.Max(0, 11 - adsrShift);
        if (isExponential && !isDecreasing && AdsrVolume > 0x6000)
        {
            envelopeCycles *= 4;
        }

        if (isExponential && isDecreasing)
        {
            envelopeStep = (envelopeStep * AdsrVolume) >> 15;
        }

        AdsrVolume  = (ushort)Math.Clamp(AdsrVolume + envelopeStep, 0, 0x7FFF);
        AdsrCounter = envelopeCycles;

        var nextPhase = isDecreasing ? AdsrVolume <= adsrTarget : AdsrVolume >= adsrTarget;
        if (nextPhase && AdsrPhase != SPUVoicePhase.Sustain)
        {
            AdsrPhase++;
            AdsrCounter = 0;
        }

        ;
    }
}