namespace ProjectPSX.Devices.CdRom;

public interface ISector
{
    void fillWith(Span<byte> data);
    ref byte readByte();
    ref short readShort();
    Span<uint> read(int size);
    Span<byte> read();
    bool hasData();
    bool hasSamples();
    void clear();
}