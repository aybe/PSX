namespace PSX.Devices.Input;

public sealed class DigitalController : Controller
{
    protected override ushort ControllerType { get; } = 0x5A41;

    protected override void GenerateResponse()
    {
        base.GenerateResponse();

        var b2 = (byte)((Buttons >> 0) & 0xFF);
        var b3 = (byte)((Buttons >> 8) & 0xFF);

        TransferDataFifo.Enqueue(b2);
        TransferDataFifo.Enqueue(b3);
    }
}