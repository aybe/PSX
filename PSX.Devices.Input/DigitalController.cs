namespace PSX.Devices.Input;

public sealed class DigitalController : Controller
{
    private const ushort CONTROLLER_TYPE = 0x5A41; //digital

    private DigitalControllerMode Mode = DigitalControllerMode.Idle;

    public override byte Process(byte b)
    {
        switch (Mode)
        {
            case DigitalControllerMode.Idle:
                switch (b)
                {
                    case 0x01:
                        //Console.WriteLine("[Controller] Idle Process 0x01");
                        Mode = DigitalControllerMode.Connected;
                        ACK  = true;
                        return 0xFF;
                    default:
                        //Console.WriteLine($"[Controller] Idle Process Warning: {b:x2}");
                        TransferDataFifo.Clear();
                        ACK = false;
                        return 0xFF;
                }

            case DigitalControllerMode.Connected:
                switch (b)
                {
                    case 0x42:
                        //Console.WriteLine("[Controller] Connected Init Transfer Process 0x42");
                        Mode = DigitalControllerMode.Transferring;
                        GenerateResponse();
                        ACK = true;
                        return TransferDataFifo.Dequeue();
                    default:
                        //Console.WriteLine("[Controller] Connected Transfer Process unknow command {b:x2} RESET TO IDLE");
                        Mode = DigitalControllerMode.Idle;
                        TransferDataFifo.Clear();
                        ACK = false;
                        return 0xFF;
                }

            case DigitalControllerMode.Transferring:
                var data = TransferDataFifo.Dequeue();
                ACK = TransferDataFifo.Count > 0;
                if (!ACK)
                {
                    //Console.WriteLine("[Controller] Changing to idle");
                    Mode = DigitalControllerMode.Idle;
                }

                //Console.WriteLine($"[Controller] Transfer Process value:{b:x2} response: {data:x2} queueCount: {transferDataFifo.Count} ACK: {ACK}");
                return data;
            default:
                //Console.WriteLine("[Controller] This should be unreachable");
                return 0xFF;
        }
    }

    public void GenerateResponse()
    {
        const byte b0 = CONTROLLER_TYPE & 0xFF;
        const byte b1 = (CONTROLLER_TYPE >> 8) & 0xFF;

        var b2 = (byte)(Buttons & 0xFF);
        var b3 = (byte)((Buttons >> 8) & 0xFF);

        TransferDataFifo.Enqueue(b0);
        TransferDataFifo.Enqueue(b1);
        TransferDataFifo.Enqueue(b2);
        TransferDataFifo.Enqueue(b3);
    }

    public override void ResetToIdle()
    {
        Mode = DigitalControllerMode.Idle;
    }
}