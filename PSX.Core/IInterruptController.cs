namespace ProjectPSX.Devices;

public interface IInterruptController
{
    void set(Interrupt interrupt);
    bool interruptPending();
    void write(uint addr, uint value);
    uint load(uint addr);
}