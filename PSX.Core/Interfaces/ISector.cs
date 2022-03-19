namespace PSX.Core.Interfaces;

public interface ISector
{
    void Clear();

    void FillWith(Span<byte> data);

    bool HasData();

    bool HasSamples();

    Span<byte> Read();

    Span<uint> Read(int size);

    ref byte ReadByte();

    ref short ReadShort();
}