namespace ProjectPSX;

public enum Interrupt {
    VBLANK = 0x1,
    GPU    = 0x2,
    CDROM  = 0x4,
    DMA    = 0x8,
    TIMER0 = 0x10,
    TIMER1 = 0x20,
    TIMER2 = 0x40,
    CONTR  = 0x80,
    SIO    = 0x100,
    SPU    = 0x200,
    PIO    = 0x400
}