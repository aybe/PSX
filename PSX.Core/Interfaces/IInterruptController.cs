namespace PSX.Core.Interfaces;

public interface IInterruptController
{
    void Set(Interrupt interrupt);

    bool InterruptPending();

    uint Load(uint address);

    void Write(uint address, uint value);
}