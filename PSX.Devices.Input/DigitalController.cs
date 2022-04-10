namespace PSX.Devices.Input;

public sealed class DigitalController : ControllerBase
{
    public DigitalController(IControllerSource source) : base(source)
    {
    }

    protected override ushort Type { get; } = 0x5A41;

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
        { InputAction.R1, default },
        { InputAction.R2, default }
    };

    protected override void GenerateResponse()
    {
        base.GenerateResponse();

        const byte pressed  = 0;
        const byte released = 1;

        var b00 = InputActions[InputAction.Select] > 0.0f ? pressed : released;
        var b01 = (byte)1; //InputActions[InputAction.L3] > 0.0f ? pressed : released;
        var b02 = (byte)1; //InputActions[InputAction.R3] > 0.0f ? pressed : released;
        var b03 = InputActions[InputAction.Start] > 0.0f ? pressed : released;
        var b04 = InputActions[InputAction.DPadUp] > 0.0f ? pressed : released;
        var b05 = InputActions[InputAction.DPadRight] > 0.0f ? pressed : released;
        var b06 = InputActions[InputAction.DPadDown] > 0.0f ? pressed : released;
        var b07 = InputActions[InputAction.DPadLeft] > 0.0f ? pressed : released;

        var b08 = InputActions[InputAction.L2] > 0.0f ? pressed : released;
        var b09 = InputActions[InputAction.R2] > 0.0f ? pressed : released;
        var b10 = InputActions[InputAction.L1] > 0.0f ? pressed : released;
        var b11 = InputActions[InputAction.R1] > 0.0f ? pressed : released;
        var b12 = InputActions[InputAction.Triangle] > 0.0f ? pressed : released;
        var b13 = InputActions[InputAction.Circle] > 0.0f ? pressed : released;
        var b14 = InputActions[InputAction.Cross] > 0.0f ? pressed : released;
        var b15 = InputActions[InputAction.Square] > 0.0f ? pressed : released;

        var b0 = (byte)0;

        b0 |=  b07;
        b0 <<= 1;
        b0 |=  b06;
        b0 <<= 1;
        b0 |=  b05;
        b0 <<= 1;
        b0 |=  b04;
        b0 <<= 1;
        b0 |=  b03;
        b0 <<= 1;
        b0 |=  b02;
        b0 <<= 1;
        b0 |=  b01;
        b0 <<= 1;
        b0 |=  b00;

        var b1 = (byte)0;

        b1 |=  b15;
        b1 <<= 1;
        b1 |=  b14;
        b1 <<= 1;
        b1 |=  b13;
        b1 <<= 1;
        b1 |=  b12;
        b1 <<= 1;
        b1 |=  b11;
        b1 <<= 1;
        b1 |=  b10;
        b1 <<= 1;
        b1 |=  b09;
        b1 <<= 1;
        b1 |=  b08;

        TransferDataFifo.Enqueue(b0);
        TransferDataFifo.Enqueue(b1);
    }
}