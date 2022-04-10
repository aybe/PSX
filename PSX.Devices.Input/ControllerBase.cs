namespace PSX.Devices.Input;

public abstract class ControllerBase : IController
{
    protected readonly Queue<byte> TransferDataFifo = new();

    private ControllerMode Mode = ControllerMode.Idle;

    protected ControllerBase(IControllerSource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    private IControllerSource Source { get; }

    protected abstract ushort Type { get; }

    protected abstract Dictionary<InputAction, float> InputActions { get; }

    public bool ACK { get; private set; }

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

    public virtual void Update()
    {
        Source.Update();

        foreach (var key in InputActions.Keys)
        {
            InputActions[key] = Source.GetValue(key);
        }
    }

    protected virtual void GenerateResponse()
    {
        TransferDataFifo.Enqueue((byte)((Type >> 0) & 0xFF));
        TransferDataFifo.Enqueue((byte)((Type >> 8) & 0xFF));
    }

    private enum ControllerMode
    {
        Idle,
        Connected,
        Transferring
    }
}