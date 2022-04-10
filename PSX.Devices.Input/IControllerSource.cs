namespace PSX.Devices.Input;

public interface IControllerSource
{
    float GetValue(InputAction inputAction);

    void Update();
}