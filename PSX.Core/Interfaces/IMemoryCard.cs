namespace PSX.Core.Interfaces;

public interface IMemoryCard
{
    bool Ack { get; }

    byte Process(byte value);

    void ResetToIdle();
}