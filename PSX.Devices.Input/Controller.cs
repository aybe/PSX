namespace PSX.Devices.Input;

public abstract class Controller
{
    protected readonly Queue<byte> TransferDataFifo = new();

    public bool ACK;

    protected ushort Buttons = 0xFFFF;

    private ControllerMode Mode = ControllerMode.Idle;

    protected abstract ushort ControllerType { get; }

    public void HandleJoyPadDown(KeyboardInput inputCode) // TODO delete
    {
        Buttons &= (ushort)~(Buttons & (ushort)inputCode);
        //Console.WriteLine(buttons.ToString("x8"));
    }

    public void HandleJoyPadUp(KeyboardInput inputCode) // TODO delete
    {
        Buttons |= (ushort)inputCode;
        //Console.WriteLine(buttons.ToString("x8"));
    }

    protected virtual void GenerateResponse()
    {
        var b0 = (byte)((ControllerType >> 0) & 0xFF);
        var b1 = (byte)((ControllerType >> 8) & 0xFF);

        TransferDataFifo.Enqueue(b0);
        TransferDataFifo.Enqueue(b1);
    }

    public virtual byte Process(byte b)
    {
        switch (Mode)
        {
            case ControllerMode.Idle:
                switch (b)
                {
                    case 0x01:
                        //Console.WriteLine("[Controller] Idle Process 0x01");
                        Mode = ControllerMode.Connected;
                        ACK  = true;
                        return 0xFF;
                    default:
                        //Console.WriteLine($"[Controller] Idle Process Warning: {b:x2}");
                        TransferDataFifo.Clear();
                        ACK = false;
                        return 0xFF;
                }

            case ControllerMode.Connected:
                switch (b)
                {
                    case 0x42:
                        //Console.WriteLine("[Controller] Connected Init Transfer Process 0x42");
                        Mode = ControllerMode.Transferring;
                        GenerateResponse();
                        ACK = true;
                        return TransferDataFifo.Dequeue();
                    default:
                        //Console.WriteLine("[Controller] Connected Transfer Process unknown command {b:x2} RESET TO IDLE");
                        Mode = ControllerMode.Idle;
                        TransferDataFifo.Clear();
                        ACK = false;
                        return 0xFF;
                }

            case ControllerMode.Transferring:
                var data = TransferDataFifo.Dequeue();
                ACK = TransferDataFifo.Count > 0;

                if (!ACK)
                {
                    //Console.WriteLine("[Controller] Changing to idle");
                    Mode = ControllerMode.Idle;
                }

                //Console.WriteLine($"[Controller] Transfer Process value:{b:x2} response: {data:x2} queueCount: {transferDataFifo.Count} ACK: {ACK}");
                return data;
            default:
                //Console.WriteLine("[Controller] This should be unreachable");
                return 0xFF;
        }
    }

    public virtual void ResetToIdle()
    {
        Mode = ControllerMode.Idle;
    }

    private enum ControllerMode
    {
        Idle,
        Connected,
        Transferring
    }
}