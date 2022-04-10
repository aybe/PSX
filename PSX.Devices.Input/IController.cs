namespace PSX.Devices.Input;

public interface IController
{
    bool ACK { get; }

    byte Process(byte b);

    void ResetToIdle();

    void Update();
}