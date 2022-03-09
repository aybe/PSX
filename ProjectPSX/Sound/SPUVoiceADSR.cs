namespace ProjectPSX.Devices.Spu;

public struct SPUVoiceADSR
{
    public ushort lo; //8
   
    public ushort hi; //A

    public bool isAttackModeExponential => ((lo >> 15) & 0x1) != 0;

    public int attackShift => (lo >> 10) & 0x1F;

    public int attackStep => (lo >> 8) & 0x3; //"+7,+6,+5,+4"

    public int decayShift => (lo >> 4) & 0xF;

    public int sustainLevel => lo & 0xF; //Level=(N+1)*800h

    public bool isSustainModeExponential => ((hi >> 15) & 0x1) != 0;

    public bool isSustainDirectionDecrease => ((hi >> 14) & 0x1) != 0;

    public int sustainShift => (hi >> 8) & 0x1F;

    public int sustainStep => (hi >> 6) & 0x3;

    public bool isReleaseModeExponential => ((hi >> 5) & 0x1) != 0;

    public int releaseShift => hi & 0x1F;
}