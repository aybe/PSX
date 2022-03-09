using System;

namespace ProjectPSX.Devices.Spu;

public class SPUVoice
{
    public ushort adpcmRepeatAddress; //E

    public SPUVoiceADSR adsr;

    private int adsrCounter;

    public SPUVoicePhase adsrPhase;

    public ushort adsrVolume; //C

    public SPUVoiceCounter counter;
    public ushort currentAddress; //6 Internal
    public short[] decodedSamples = new short[31]; //28 samples from current block + 3 to make room for interpolation

    public bool hasSamples;

    public short latest;

    public short old;
    public short older;

    public ushort pitch; //4

    public bool readRamIrq;

    public byte[] spuAdpcm = new byte[16];
    public ushort startAddress; //6

    public SPUVoiceVolume volumeLeft; //0
    public SPUVoiceVolume volumeRight; //2

    public SPUVoice()
    {
        adsrPhase = SPUVoicePhase.Off;
    }

    private static ReadOnlySpan<sbyte> positiveXaAdpcmTable => new sbyte[] { 0, 60, 115, 98, 122 };

    private static ReadOnlySpan<sbyte> negativeXaAdpcmTable => new sbyte[] { 0, 0, -52, -55, -60 };

    public void keyOn()
    {
        hasSamples     = false;
        old            = 0;
        older          = 0;
        currentAddress = startAddress;
        adsrCounter    = 0;
        adsrVolume     = 0;
        adsrPhase      = SPUVoicePhase.Attack;
    }

    public void keyOff()
    {
        adsrCounter = 0;
        adsrPhase   = SPUVoicePhase.Release;
    }

    internal void decodeSamples(byte[] ram, ushort ramIrqAddress)
    {
        //save the last 3 samples from the last decoded block
        //this are needed for interpolation in case the voice.counter.currentSampleIndex is 0 1 or 2
        decodedSamples[2] = decodedSamples[decodedSamples.Length - 1];
        decodedSamples[1] = decodedSamples[decodedSamples.Length - 2];
        decodedSamples[0] = decodedSamples[decodedSamples.Length - 3];

        Array.Copy(ram, currentAddress * 8, spuAdpcm, 0, 16);

        //ramIrqAddress is >> 8 so we only need to check for currentAddress and + 1
        readRamIrq |= currentAddress == ramIrqAddress || currentAddress + 1 == ramIrqAddress;

        var shift = 12 - (spuAdpcm[0] & 0x0F);
        var filter = (spuAdpcm[0] & 0x70) >> 4; //filter on SPU adpcm is 0-4 vs XA wich is 0-3
        if (filter > 4)
            filter = 4; //Crash Bandicoot sets this to 7 at the end of the first level and overflows the filter

        int f0 = positiveXaAdpcmTable[filter];
        int f1 = negativeXaAdpcmTable[filter];

        //Actual ADPCM decoding is the same as on XA but the layout here is sequencial by nibble where on XA in grouped by nibble line
        var position = 2; //skip shift and flags
        var nibble = 1;
        for (var i = 0; i < 28; i++)
        {
            nibble = (nibble + 1) & 0x1;

            var t = signed4bit((byte)((spuAdpcm[position] >> (nibble * 4)) & 0x0F));
            var s = (t << shift) + (old * f0 + older * f1 + 32) / 64;
            var sample = (short)Math.Clamp(s, -0x8000, 0x7FFF);

            decodedSamples[3 + i] = sample;

            older = old;
            old   = sample;

            position += nibble;
        }
    }

    public static int signed4bit(byte value)
    {
        return (value << 28) >> 28;
    }

    internal short processVolume(SPUVoiceVolume volume)
    {
        if (!volume.isSweepMode)
        {
            return volume.fixedVolume;
        }

        return 0; //todo handle sweep mode volume envelope
    }

    internal void tickAdsr(int v)
    {
        if (adsrPhase == SPUVoicePhase.Off)
        {
            adsrVolume = 0;
            return;
        }

        int adsrTarget;
        int adsrShift;
        int adsrStep;
        bool isDecreasing;
        bool isExponential;

        //Todo move out of tick the actual change of phase
        switch (adsrPhase)
        {
            case SPUVoicePhase.Attack:
                adsrTarget    = 0x7FFF;
                adsrShift     = adsr.attackShift;
                adsrStep      = 7 - adsr.attackStep; // reg is 0-3 but values are "+7,+6,+5,+4"
                isDecreasing  = false; // Allways increase till 0x7FFF
                isExponential = adsr.isAttackModeExponential;
                break;
            case SPUVoicePhase.Decay:
                adsrTarget    = (adsr.sustainLevel + 1) * 0x800;
                adsrShift     = adsr.decayShift;
                adsrStep      = -8;
                isDecreasing  = true; // Allways decreases (till target)
                isExponential = true; // Allways exponential
                break;
            case SPUVoicePhase.Sustain:
                adsrTarget    = 0;
                adsrShift     = adsr.sustainShift;
                adsrStep      = adsr.isSustainDirectionDecrease ? -8 + adsr.sustainStep : 7 - adsr.sustainStep;
                isDecreasing  = adsr.isSustainDirectionDecrease; //till keyoff
                isExponential = adsr.isSustainModeExponential;
                break;
            case SPUVoicePhase.Release:
                adsrTarget    = 0;
                adsrShift     = adsr.releaseShift;
                adsrStep      = -8;
                isDecreasing  = true; // Allways decrease till 0
                isExponential = adsr.isReleaseModeExponential;
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

        if (adsrCounter > 0)
        {
            adsrCounter--;
            return;
        }

        var envelopeCycles = 1 << Math.Max(0, adsrShift - 11);
        var envelopeStep = adsrStep << Math.Max(0, 11 - adsrShift);
        if (isExponential && !isDecreasing && adsrVolume > 0x6000)
        {
            envelopeCycles *= 4;
        }

        if (isExponential && isDecreasing)
        {
            envelopeStep = (envelopeStep * adsrVolume) >> 15;
        }

        adsrVolume  = (ushort)Math.Clamp(adsrVolume + envelopeStep, 0, 0x7FFF);
        adsrCounter = envelopeCycles;

        var nextPhase = isDecreasing ? adsrVolume <= adsrTarget : adsrVolume >= adsrTarget;
        if (nextPhase && adsrPhase != SPUVoicePhase.Sustain)
        {
            adsrPhase++;
            adsrCounter = 0;
        }

        ;
    }
}