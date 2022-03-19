namespace PSX.Core;

public interface IMemoryCard
{
    bool ACK { get; }

    byte Process(byte value);

    void ResetToIdle();
}