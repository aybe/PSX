namespace PSX.Core;

public interface IInterruptController
{
    void set(Interrupt interrupt);
    bool interruptPending();
    void write(uint addr, uint value);
    uint load(uint addr);
}