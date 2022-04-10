namespace PSX.Devices.Input;

public interface IController
{
    bool ACK { get; }

    ushort Type { get; }

    void GenerateResponse();

    byte Process(byte b);

    void ResetToIdle();
}