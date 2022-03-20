namespace PSX.Core.Sound.Internal;

internal struct VoiceADSR
{
    public ushort Lo; // 8

    public ushort Hi; // A

    public bool IsAttackModeExponential => ((Lo >> 15) & 0x1) != 0;

    public int AttackShift => (Lo >> 10) & 0x1F;

    public int AttackStep => (Lo >> 8) & 0x3; // "+7,+6,+5,+4"

    public int DecayShift => (Lo >> 4) & 0xF;

    public int SustainLevel => Lo & 0xF; // Level=(N+1)*800h

    public bool IsSustainModeExponential => ((Hi >> 15) & 0x1) != 0;

    public bool IsSustainDirectionDecrease => ((Hi >> 14) & 0x1) != 0;

    public int SustainShift => (Hi >> 8) & 0x1F;

    public int SustainStep => (Hi >> 6) & 0x3;

    public bool IsReleaseModeExponential => ((Hi >> 5) & 0x1) != 0;

    public int ReleaseShift => Hi & 0x1F;
}