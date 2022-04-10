namespace PSX.Devices.Input;

public abstract class BaseControllerSource : IControllerSource
{
    public abstract float GetValue(InputAction inputAction);

    public abstract void Update();
}