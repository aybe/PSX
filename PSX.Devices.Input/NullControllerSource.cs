namespace PSX.Devices.Input;

public sealed class NullControllerSource : IControllerSource
{
    public float GetValue(InputAction inputAction)
    {
        return default;
    }

    public void Update()
    {
    }
}