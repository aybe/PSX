namespace PSX.Devices.Input;

public sealed class NullController : ControllerBase
{
    private const int HiZ = 0xFF;

    public override ushort Type { get; } = 0xFFFF;

    public override void GenerateResponse()
    {
        base.GenerateResponse();

        for (var i = 0; i < 15; i++)
        {
            TransferDataFifo.Enqueue(HiZ);
        }
    }
}