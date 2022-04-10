using PSX.Core.Interfaces;

// ReSharper disable CommentTypo

namespace PSX.Devices.Input;

// ReSharper disable once InconsistentNaming
public class JOYPAD
// BUG add MC #2
{
    private readonly IMemoryCard MemoryCard;

    private IController Controller
    {
        get
        {
            return DesiredSlotNumber switch
            {
                0 => Controller1,
                1 => Controller2,
                _ => throw new InvalidOperationException($"{nameof(DesiredSlotNumber)}: {DesiredSlotNumber}")
            };
        }
    }

    public IController Controller1, Controller2;

    private int Counter;

    private ushort JOY_BAUD; //1F80104Eh JOY_BAUD(R/W) (usually 0088h, ie.circa 250kHz, when Factor = MUL1)

    private byte JOY_RX_DATA; // 1F801040h JOY_RX_DATA(R) FIFO

    private byte JOY_TX_DATA; // 1F801040h JOY_TX_DATA(W)

    private JoypadDevice JoypadDevice = JoypadDevice.None;

    public JOYPAD(IController controller1, IController controller2, IMemoryCard memoryCard)
    {
        Controller1 = controller1;
        Controller2 = controller2;
        MemoryCard  = memoryCard;
    }

    public bool Tick()
    {
        if (Counter > 0)
        {
            Counter -= 100;

            if (Counter == 0)
            {
                //Console.WriteLine("[IRQ] TICK Triggering JOYPAD");
                ACKInputLevel    = false;
                InterruptRequest = true;
            }
        }

        if (InterruptRequest)
            return true;

        return false;
    }

    private void ReloadTimer()
    {
        //Console.WriteLine("[JOYPAD] RELOAD TIMER");
        BaudRateTimer = (int)(JOY_BAUD * BaudRateReloadFactor) & ~0x1;
    }

    public void Write(uint address, uint value)
    {
        switch (address & 0xFF)
        {
            case 0x40:
                //Console.WriteLine("[JOYPAD] TX DATA ENQUEUE " + value.ToString("x2"));
                JOY_TX_DATA = (byte)value;
                JOY_RX_DATA = 0xFF;
                FifoFull    = true;

                TXReadyFlag1 = true;
                TXReadyFlag2 = false;

                var controller = Controller;

                if (JoyOutput)
                {
                    TXReadyFlag2 = true;

                    //Console.WriteLine("[JOYPAD] DesiredSlot == " + desiredSlotNumber);

                    if (true)
                    {
                        if (JoypadDevice == JoypadDevice.None)
                        {
                            //Console.ForegroundColor = ConsoleColor.Red;
                            if (value == 0x01)
                            {
                                //Console.ForegroundColor = ConsoleColor.Green;
                                JoypadDevice = JoypadDevice.Controller;
                            }
                            else if (value == 0x81)
                            {
                                //Console.ForegroundColor = ConsoleColor.Blue;
                                JoypadDevice = JoypadDevice.MemoryCard;
                            }
                        }

                        if (JoypadDevice == JoypadDevice.Controller)
                        {
                            JOY_RX_DATA   = controller.Process(JOY_TX_DATA);
                            ACKInputLevel = controller.ACK;
                            if (ACKInputLevel)
                                Counter = 500;
                            //Console.WriteLine($"[JOYPAD] Controller TICK Enqueued RX response {JOY_RX_DATA:x2} ACK: {ackInputLevel}");
                            //Console.ReadLine();
                        }
                        else if (JoypadDevice == JoypadDevice.MemoryCard)
                        {
                            JOY_RX_DATA   = MemoryCard.Process(JOY_TX_DATA);
                            ACKInputLevel = MemoryCard.Ack;
                            if (ACKInputLevel)
                                Counter = 500;
                            //Console.WriteLine($"[JOYPAD] MemCard TICK Enqueued RX response {JOY_RX_DATA:x2} ACK: {ackInputLevel}");
                            //Console.ReadLine();
                        }
                        else
                        {
                            ACKInputLevel = false;
                        }

                        if (ACKInputLevel == false)
                            JoypadDevice = JoypadDevice.None;
                    }
                }
                else
                {
                    JoypadDevice = JoypadDevice.None;
                    MemoryCard.ResetToIdle();
                    controller.ResetToIdle();

                    ACKInputLevel = false;
                }

                break;
            case 0x48:
                //Console.WriteLine($"[JOYPAD] SET MODE {value:x4}");
                SetJoyMode(value);
                break;
            case 0x4A:
                //Console.WriteLine($"[JOYPAD] SET CONTROL {value:x4}");
                SetJoyCtrl(value);
                break;
            case 0x4E:
                //Console.WriteLine($"[JOYPAD] SET BAUD {value:x4}");
                JOY_BAUD = (ushort)value;
                ReloadTimer();
                break;
            default:
                Console.WriteLine($"Unhandled JOYPAD Write {address:x8} {value:x8}");
                //Console.ReadLine();
                break;
        }
    }

    public uint Load(uint address)
    {
        switch (address & 0xFF)
        {
            case 0x40:
                //Console.WriteLine($"[JOYPAD] GET RX DATA {JOY_RX_DATA:x2}");
                FifoFull = false;
                return JOY_RX_DATA;
            case 0x44:
                //Console.WriteLine($"[JOYPAD] GET STAT {GetJoyStat():x8}");
                return GetJoyStat();
            case 0x48:
                //Console.WriteLine($"[JOYPAD] GET MODE {GetJoyMode():x8}");
                return GetJoyMode();
            case 0x4A:
                //Console.WriteLine($"[JOYPAD] GET CONTROL {GetJoyCtrl():x8}");
                return GetJoyCtrl();
            case 0x4E:
                //Console.WriteLine($"[JOYPAD] GET BAUD {JOY_BAUD:x8}");
                return JOY_BAUD;
            default:
                //Console.WriteLine($"[JOYPAD] Unhandled Read at {address}"); Console.ReadLine();
                return 0xFFFF_FFFF;
        }
    }

    #region 1F801044h JOY_STAT (R)

    private uint GetJoyStat()
    {
        uint joyStat = 0;

        joyStat |= TXReadyFlag1 ? 1u : 0u;
        joyStat |= (FifoFull ? 1u : 0u) << 1;
        joyStat |= (TXReadyFlag2 ? 1u : 0u) << 2;
        joyStat |= (RXParityError ? 1u : 0u) << 3;
        joyStat |= (ACKInputLevel ? 1u : 0u) << 7;
        joyStat |= (InterruptRequest ? 1u : 0u) << 9;
        joyStat |= (uint)BaudRateTimer << 11;

        ACKInputLevel = false;

        return joyStat;
    }

    private bool TXReadyFlag1 = true;
    private bool FifoFull;
    private bool TXReadyFlag2 = true;
    private bool RXParityError;
    private bool ACKInputLevel;
    private bool InterruptRequest;
    private int  BaudRateTimer;

    #endregion

    #region 1F801048h JOY_MODE (R/W)

    private uint GetJoyMode()
    {
        uint joyMode = 0;

        joyMode |= BaudRateReloadFactor;
        joyMode |= CharacterLength << 2;
        joyMode |= (ParityEnable ? 1u : 0u) << 4;
        joyMode |= (ParityTypeOdd ? 1u : 0u) << 5;
        joyMode |= (ClkOutputPolarity ? 1u : 0u) << 4;

        return joyMode;
    }

    private void SetJoyMode(uint value)
    {
        BaudRateReloadFactor = value & 0x3;
        CharacterLength      = (value >> 2) & 0x3;
        ParityEnable         = ((value >> 4) & 0x1) != 0;
        ParityTypeOdd        = ((value >> 5) & 0x1) != 0;
        ClkOutputPolarity    = ((value >> 8) & 0x1) != 0;
    }

    private uint BaudRateReloadFactor;
    private uint CharacterLength;
    private bool ParityEnable;
    private bool ParityTypeOdd;
    private bool ClkOutputPolarity;

    #endregion

    #region 1F80104Ah JOY_CTRL (R/W)

    private uint GetJoyCtrl()
    {
        uint joyCtrl = 0;
        joyCtrl |= TXEnable ? 1u : 0u;
        joyCtrl |= (JoyOutput ? 1u : 0u) << 1;
        joyCtrl |= (RXEnable ? 1u : 0u) << 2;
        joyCtrl |= (JoyControlUnknownBit3 ? 1u : 0u) << 3;
        //joy_ctrl |= (ACK ? 1u : 0u) << 4; // only writable
        joyCtrl |= (JoyControlUnknownBit5 ? 1u : 0u) << 5;
        //joy_ctrl |= (reset ? 1u : 0u) << 6; // only writable
        //bit 7 always 0
        joyCtrl |= RXInterruptMode << 8;
        joyCtrl |= (TXInterruptEnable ? 1u : 0u) << 10;
        joyCtrl |= (RXInterruptEnable ? 1u : 0u) << 11;
        joyCtrl |= (ACKInterruptEnable ? 1u : 0u) << 12;
        joyCtrl |= DesiredSlotNumber << 13;
        return joyCtrl;
    }

    private void SetJoyCtrl(uint value)
    {
        TXEnable              = (value & 0x1) != 0;
        JoyOutput             = ((value >> 1) & 0x1) != 0;
        RXEnable              = ((value >> 2) & 0x1) != 0;
        JoyControlUnknownBit3 = ((value >> 3) & 0x1) != 0;
        ControlAck            = ((value >> 4) & 0x1) != 0;
        JoyControlUnknownBit5 = ((value >> 5) & 0x1) != 0;
        ControlReset          = ((value >> 6) & 0x1) != 0;
        RXInterruptMode       = (value >> 8) & 0x3;
        TXInterruptEnable     = ((value >> 10) & 0x1) != 0;
        RXInterruptEnable     = ((value >> 11) & 0x1) != 0;
        ACKInterruptEnable    = ((value >> 12) & 0x1) != 0;
        DesiredSlotNumber     = (value >> 13) & 0x1;

        if (ControlAck)
        {
            //Console.WriteLine("[JOYPAD] CONTROL ACK");
            RXParityError    = false;
            InterruptRequest = false;
            ControlAck       = false;
        }

        if (ControlReset)
        {
            //Console.WriteLine("[JOYPAD] CONTROL RESET");
            JoypadDevice = JoypadDevice.None;
            Controller.ResetToIdle();
            MemoryCard.ResetToIdle();
            FifoFull = false;

            SetJoyMode(0);
            SetJoyCtrl(0);
            JOY_BAUD = 0;

            JOY_RX_DATA = 0xFF;
            JOY_TX_DATA = 0xFF;

            TXReadyFlag1 = true;
            TXReadyFlag2 = true;

            ControlReset = false;
        }

        if (!JoyOutput)
        {
            JoypadDevice = JoypadDevice.None;
            MemoryCard.ResetToIdle();
            Controller.ResetToIdle();
        }
    }

    private bool TXEnable;
    private bool JoyOutput;
    private bool RXEnable;
    private bool JoyControlUnknownBit3;
    private bool ControlAck;
    private bool JoyControlUnknownBit5;
    private bool ControlReset;
    private uint RXInterruptMode;
    private bool TXInterruptEnable;
    private bool RXInterruptEnable;
    private bool ACKInterruptEnable;
    private uint DesiredSlotNumber;

    #endregion
}