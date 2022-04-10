namespace PSX.Devices.Input;

public class DualAnalogController : DigitalController
{
    public DualAnalogController(IControllerSource source) : base(source)
    {
    }

    protected override ushort Type { get; } = 0x5A73;

    protected override Dictionary<InputAction, float> InputActions { get; } = new()
    {
        { InputAction.DPadUp, default },
        { InputAction.DPadDown, default },
        { InputAction.DPadLeft, default },
        { InputAction.DPadRight, default },
        { InputAction.Triangle, default },
        { InputAction.Circle, default },
        { InputAction.Cross, default },
        { InputAction.Square, default },
        { InputAction.Select, default },
        { InputAction.Start, default },
        { InputAction.L1, default },
        { InputAction.L2, default },
        { InputAction.L3, default },
        { InputAction.R1, default },
        { InputAction.R2, default },
        { InputAction.R3, default },
        { InputAction.LeftX, default },
        { InputAction.LeftY, default },
        { InputAction.RightX, default },
        { InputAction.RightY, default }
    };

    protected override void GenerateResponse()
    {
        base.GenerateResponse();

        var rx = InputActions[InputAction.RightX];
        var ry = InputActions[InputAction.RightY];
        var lx = InputActions[InputAction.LeftX];
        var ly = InputActions[InputAction.LeftY];

        TransferDataFifo.Enqueue((byte)rx);
        TransferDataFifo.Enqueue((byte)ry);
        TransferDataFifo.Enqueue((byte)lx);
        TransferDataFifo.Enqueue((byte)ly);
    }
}