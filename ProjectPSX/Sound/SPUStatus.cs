namespace ProjectPSX.Devices;

internal struct SPUStatus
{
    public ushort register;

    public bool isSecondHalfCaptureBuffer => ((register >> 11) & 0x1) != 0;

    public bool dataTransferBusyFlag => ((register >> 10) & 0x1) != 0;

    public bool dataTransferDmaReadRequest => ((register >> 9) & 0x1) != 0;

    public bool dataTransferDmaWriteRequest => ((register >> 8) & 0x1) != 0;

    //  7     Data Transfer DMA Read/Write Request ;seems to be same as SPUCNT.Bit5 todo
    public bool irq9Flag
    {
        get => ((register >> 6) & 0x1) != 0;
        set => register = value ? (ushort)(register | (1 << 6)) : (ushort)(register & ~(1 << 6));
    }
}