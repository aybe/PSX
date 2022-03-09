using System.Collections.Generic;

namespace ProjectPSX.Input;

public abstract class Controller
{
    protected readonly Queue<byte> TransferDataFifo = new();

    public bool ACK;

    protected ushort Buttons = 0xFFFF;

    public abstract byte Process(byte b);

    public abstract void ResetToIdle();

    public void HandleJoyPadDown(KeyboardInput inputCode)
    {
        Buttons &= (ushort)~(Buttons & (ushort)inputCode);
        //Console.WriteLine(buttons.ToString("x8"));
    }

    public void HandleJoyPadUp(KeyboardInput inputCode)
    {
        Buttons |= (ushort)inputCode;
        //Console.WriteLine(buttons.ToString("x8"));
    }
}