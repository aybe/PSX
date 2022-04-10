namespace PSX.Devices.Input;

public sealed class NullController : ControllerBase
{
    private const int HiZ = 0xFF;

    public NullController(IControllerSource source) : base(source)
    {
    }

    protected override ushort Type { get; } = 0xFFFF;

    protected override Dictionary<InputAction, float> InputActions { get; } = new();

    protected override void GenerateResponse()
    {
        base.GenerateResponse();

        for (var i = 0; i < 15; i++)
        {
            TransferDataFifo.Enqueue(HiZ);
        }
    }
}