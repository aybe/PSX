using System.Diagnostics.CodeAnalysis;
using PSX.Core.Interfaces;

namespace PSX.Core;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
public sealed class InterruptController : IInterruptController
{
    private uint IMASK; //IE Global Interrupt enable

    private uint ISTAT; //IF Trigger that needs to be ACK

    public void Set(Interrupt interrupt)
    {
        ISTAT |= (uint)interrupt;
        //Console.WriteLine($"ISTAT SET MANUAL FROM DEVICE: {ISTAT:x8} IMASK {IMASK:x8}");
    }

    public bool InterruptPending()
    {
        return (ISTAT & IMASK) != 0;
    }

    public uint Load(uint address)
    {
        var register = address & 0xF;

        if (register == 0)
        {
            return ISTAT;
        }

        if (register == 4)
        {
            return IMASK;
        }

        return 0xFFFF_FFFF;
    }

    public void Write(uint address, uint value)
    {
        var register = address & 0xF;

        if (register == 0)
        {
            ISTAT &= value & 0x7FF;
        }
        else if (register == 4)
        {
            IMASK = value & 0x7FF;
        }
    }


    internal uint LoadIMASK()
    {
        //Console.WriteLine($"[IRQ] [IMASK] Load {IMASK:x8}");
        //Console.ReadLine();
        return IMASK;
    }

    internal uint LoadISTAT()
    {
        //Console.WriteLine($"[IRQ] [ISTAT] Load {ISTAT:x8}");
        //Console.ReadLine();
        return ISTAT;
    }

    internal void WriteIMASK(uint value)
    {
        IMASK = value & 0x7FF;
        //Console.WriteLine($"[IRQ] [IMASK] Write {IMASK:x8}");
        //Console.ReadLine();
    }

    internal void WriteISTAT(uint value)
    {
        ISTAT &= value & 0x7FF;
        //Console.ForegroundColor = ConsoleColor.Magenta;
        //Console.WriteLine($"[IRQ] [ISTAT] Write {value:x8} ISTAT {ISTAT:x8}");
        //Console.ResetColor();
        //Console.ReadLine();
    }
}