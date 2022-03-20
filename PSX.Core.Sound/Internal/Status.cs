namespace PSX.Core.Sound.Internal;

internal struct Status
{
    public ushort Register;

    public bool IsSecondHalfCaptureBuffer => ((Register >> 11) & 0x1) != 0;

    public bool DataTransferBusyFlag => ((Register >> 10) & 0x1) != 0;

    public bool DataTransferDmaReadRequest => ((Register >> 9) & 0x1) != 0;

    public bool DataTransferDmaWriteRequest => ((Register >> 8) & 0x1) != 0;

    //  7     Data Transfer DMA Read/Write Request ;seems to be same as SPUCNT.Bit5 todo
    public bool Irq9Flag
    {
        get => ((Register >> 6) & 0x1) != 0;
        set => Register = value ? (ushort)(Register | (1 << 6)) : (ushort)(Register & ~(1 << 6));
    }
}