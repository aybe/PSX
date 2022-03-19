using System;
using System.IO;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace ProjectPSX.Storage;

public class MemoryCard
{
    //emulating a 3rd party one as it seems easier to and 0x3FF bad address than to handle the
    //original memcard badAddress 0xFFFF error and the IdCommand
    private const byte MEMORY_CARD_ID_1 = 0x5A;

    private const byte MEMORY_CARD_ID_2 = 0x5D;

    private const byte MEMORY_CARD_COMMAND_ACK_1 = 0x5C;

    private const byte MEMORY_CARD_COMMAND_ACK_2 = 0x5D;

    //FLAG
    //only bit 2 (isError) and 3 (isNotReaded) seems documented
    //bit 5 is useless for non sony memcards, default value is 0x80
    private const byte FLAG_ERROR = 0x4;

    private const byte FLAG_NOT_READ = 0x8;

    private const string MemCardFilePath = "./memcard.mcr";

    private readonly byte[] Memory = new byte[128 * 1024]; //Standard memcard 128KB

    public bool ACK;

    private ushort Address;

    private byte AddressLSB;

    private byte AddressMSB;

    private byte Checksum;

    private byte EndTransfer;

    private byte Flag = 0x8;

    private MemoryCardMode Mode = MemoryCardMode.Idle;

    private int ReadPointer;

    private MemoryCardTransferMode TransferMode = MemoryCardTransferMode.Undefined;

    public MemoryCard()
    {
        try
        {
            Memory = File.ReadAllBytes(MemCardFilePath);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[MemCard] File found. Contents Loaded.");
            Console.ResetColor();
        }
        catch (Exception)
        {
            Console.WriteLine("[MemCard] No Card found. Will try to generate a new one on save.");
        }
    }

    public byte Process(byte value)
        //This should be handled with some early response and post address queues but atm its easier to handle as a state machine
    {
        //Console.WriteLine($"[MemCard] rawProcess {value:x2} previous ACK {ACK}");
        switch (TransferMode)
        {
            case MemoryCardTransferMode.Read: return ReadMemory(value);
            case MemoryCardTransferMode.Write: return WriteMemory(value);
            case MemoryCardTransferMode.Id: return 0xFF;
        }

        switch (Mode)
        {
            case MemoryCardMode.Idle:
                switch (value)
                {
                    case 0x81:
                        //Console.WriteLine("[MemCard] Idle Process 0x81");
                        Mode = MemoryCardMode.Transfer;
                        ACK  = true;
                        return 0xFF;
                    default:
                        //Console.WriteLine("[MemCard] Idle value WARNING " + value);
                        ACK = false;
                        return 0xFF;
                }

            case MemoryCardMode.Transfer:
                switch (value)
                {
                    case 0x52: //Read
                        //Console.WriteLine("[MemCard] Read Process 0x52");
                        TransferMode = MemoryCardTransferMode.Read;
                        break;
                    case 0x57: //Write
                        //Console.WriteLine("[MemCard] Write Process 0x57");
                        TransferMode = MemoryCardTransferMode.Write;
                        break;
                    case 0x53: //ID
                        //Console.WriteLine("[MemCard] ID Process 0x53");
                        TransferMode = MemoryCardTransferMode.Undefined;
                        break;
                    default:
                        //Console.WriteLine($"[MemCard] Unhandled Transfer Process {value:x2}");
                        TransferMode = MemoryCardTransferMode.Undefined;
                        ACK          = false;
                        return 0xFF;
                }

                var prevFlag = Flag;
                ACK  =  true;
                Flag &= unchecked((byte)~FLAG_ERROR);
                return prevFlag;

            default:
                //Console.WriteLine("[[MemCard]] Unreachable Mode Warning");
                ACK = false;
                return 0xFF;
        }
    }

    public void ResetToIdle()
    {
        ReadPointer  = 0;
        TransferMode = MemoryCardTransferMode.Undefined;
        Mode         = MemoryCardMode.Idle;
    }

    private byte ReadMemory(byte value)
        /*  Reading Data from Memory Card
            Send Reply Comment
            81h  N/A   Memory Card Access (unlike 01h=Controller access), dummy response
            52h  FLAG  Send Read Command (ASCII "R"), Receive FLAG Byte
    
            00h  5Ah   Receive Memory Card ID1
            00h  5Dh   Receive Memory Card ID2
            MSB  (00h) Send Address MSB  ;\sector number (0..3FFh)
            LSB  (pre) Send Address LSB  ;/
            00h  5Ch   Receive Command Acknowledge 1  ;<-- late /ACK after this byte-pair
            00h  5Dh   Receive Command Acknowledge 2
            00h  MSB   Receive Confirmed Address MSB
            00h  LSB   Receive Confirmed Address LSB
            00h  ...   Receive Data Sector (128 bytes)
            00h  CHK   Receive Checksum (MSB xor LSB xor Data bytes)
            00h  47h   Receive Memory End Byte (should be always 47h="G"=Good for Read)
        */
    {
        //Console.WriteLine($"[MemCard] readMemory pointer: {readPointer} value: {value:x2} ACK {ACK}");
        ACK = true;
        switch (ReadPointer++)
        {
            case 0: return MEMORY_CARD_ID_1;
            case 1: return MEMORY_CARD_ID_2;
            case 2:
                AddressMSB = (byte)(value & 0x3);
                return 0;
            case 3:
                AddressLSB = value;
                Address    = (ushort)((AddressMSB << 8) | AddressLSB);
                Checksum   = (byte)(AddressMSB ^ AddressLSB);
                return 0;
            case 4: return MEMORY_CARD_COMMAND_ACK_1;
            case 5: return MEMORY_CARD_COMMAND_ACK_2;
            case 6: return AddressMSB;
            case 7: return AddressLSB;
            //from here handle the 128 bytes of the read sector frame
            case var index when ReadPointer - 1 >= 8 && ReadPointer - 1 < 8 + 128:
                //Console.WriteLine($"Read readPointer {readPointer - 1} index {index}");
                var data = Memory[Address * 128 + (index - 8)];
                Checksum ^= data;
                return data;
            //sector frame ended after 128 bytes, handle checksum and finish
            case 8 + 128:
                return Checksum;
            case 9 + 128:
                TransferMode = MemoryCardTransferMode.Undefined;
                Mode         = MemoryCardMode.Idle;
                ReadPointer  = 0;
                ACK          = false;
                return 0x47;
            default:
                Console.WriteLine($"[MemCard] Unreachable! {ReadPointer}");
                TransferMode = MemoryCardTransferMode.Undefined;
                Mode         = MemoryCardMode.Idle;
                ReadPointer  = 0;
                ACK          = false;
                return 0xFF;
        }
    }

    private byte WriteMemory(byte value)
        /*  Writing Data to Memory Card
            Send Reply Comment
            81h  N/A   Memory Card Access (unlike 01h=Controller access), dummy response
            57h  FLAG  Send Write Command (ASCII "W"), Receive FLAG Byte
    
            00h  5Ah   Receive Memory Card ID1
            00h  5Dh   Receive Memory Card ID2
            MSB  (00h) Send Address MSB  ;\sector number (0..3FFh)
            LSB  (pre) Send Address LSB  ;/
            ...  (pre) Send Data Sector (128 bytes)
            CHK  (pre) Send Checksum (MSB xor LSB xor Data bytes)
            00h  5Ch   Receive Command Acknowledge 1
            00h  5Dh   Receive Command Acknowledge 2
            00h  4xh   Receive Memory End Byte (47h=Good, 4Eh=BadChecksum, FFh=BadSector)
        */
    {
        //Console.WriteLine($"[MemCard] writeMemory pointer: {readPointer} value: {value:x2} ACK {ACK}");
        switch (ReadPointer++)
        {
            case 0: return MEMORY_CARD_ID_1;
            case 1: return MEMORY_CARD_ID_2;
            case 2:
                AddressMSB = value;
                return 0;
            case 3:
                AddressLSB  = value;
                Address     = (ushort)((AddressMSB << 8) | AddressLSB);
                EndTransfer = 0x47; //47h=Good

                if (Address > 0x3FF)
                {
                    Flag        |= FLAG_ERROR;
                    EndTransfer =  0xFF; //FFh = BadSector
                    Address     &= 0x3FF;
                    AddressMSB  &= 0x3;
                }

                Checksum = (byte)(AddressMSB ^ AddressLSB);
                return 0;
            //from here handle the 128 bytes of the read sector frame
            case int index when ReadPointer - 1 >= 4 && ReadPointer - 1 < 4 + 128:
                //Console.WriteLine($"Write readPointer {readPointer - 1} index {index} value {value:x2}");
                Memory[Address * 128 + (index - 4)] =  value;
                Checksum                            ^= value;
                return 0;
            //sector frame ended after 128 bytes, handle checksum and finish
            case 4 + 128:
                if (Checksum != value)
                {
                    //Console.WriteLine($"MemCard Write CHECKSUM WRONG was: {checksum:x2} expected: {value:x2}");
                    Flag |= FLAG_ERROR;
                }

                return 0;
            case 5 + 128: return MEMORY_CARD_COMMAND_ACK_1;
            case 6 + 128: return MEMORY_CARD_COMMAND_ACK_2;
            case 7 + 128:
                //Console.WriteLine($"End WRITE Transfer with code {endTransfer:x2}");
                TransferMode =  MemoryCardTransferMode.Undefined;
                Mode         =  MemoryCardMode.Idle;
                ReadPointer  =  0;
                ACK          =  false;
                Flag         &= unchecked((byte)~FLAG_NOT_READ);
                HandleSave();
                return EndTransfer;

            default:
                //Console.WriteLine($"WARNING DEFAULT Write Memory readpointer ws {readPointer}");
                TransferMode = MemoryCardTransferMode.Undefined;
                Mode         = MemoryCardMode.Idle;
                ReadPointer  = 0;
                ACK          = false;
                return 0xFF;
        }
    }

    private void HandleSave()
    {
        try
        {
            File.WriteAllBytes(MemCardFilePath, Memory);
            Console.WriteLine("[MemCard] Saved");
        }
        catch (Exception e)
        {
            Console.WriteLine("[MemCard] Error trying to save memCard file\n" + e);
        }
    }

    private byte IdMemory(byte value)
        /*  Get Memory Card ID Command
            Send Reply Comment
            81h  N/A   Memory Card Access (unlike 01h=Controller access), dummy response
            53h  FLAG  Send Get ID Command (ASCII "S"), Receive FLAG Byte
    
            00h  5Ah   Receive Memory Card ID1
            00h  5Dh   Receive Memory Card ID2
            00h  5Ch   Receive Command Acknowledge 1
            00h  5Dh   Receive Command Acknowledge 2
            00h  04h   Receive 04h
            00h  00h   Receive 00h
            00h  00h   Receive 00h
            00h  80h   Receive 80h
        */
    {
        Console.WriteLine("[MEMORY CARD] WARNING Id UNHANDLED COMMAND");
        //Console.ReadLine();
        TransferMode = MemoryCardTransferMode.Undefined;
        Mode         = MemoryCardMode.Idle;
        return 0xFF;
    }
}