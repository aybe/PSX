namespace PSX.Devices.Input;

public sealed class DigitalController : ControllerBase
{
    public override ushort Type { get; } = 0x5A41;

    public override void GenerateResponse()
    {
        base.GenerateResponse();

        var b2 = (byte)((Buttons >> 0) & 0xFF);
        var b3 = (byte)((Buttons >> 8) & 0xFF);

        TransferDataFifo.Enqueue(b2);
        TransferDataFifo.Enqueue(b3);
    }
}