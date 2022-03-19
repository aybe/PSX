namespace ProjectPSX.Storage;

public interface IMemoryCard
{
    bool ACK { get; }

    byte Process(byte value);

    void ResetToIdle();
}