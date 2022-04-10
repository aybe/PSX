using PSX.Frontend.Input.XInput.Interop;
using static PSX.Frontend.Input.XInput.Interop.NativeConstants;
using static PSX.Frontend.Input.XInput.Interop.NativeMethods;

namespace PSX.Frontend.Input.XInput;

public static class XInputController
{
    private const string MsgUnknownError = "An unknown error has occurred";

    private const string MsgDeviceNotConnected = "The device is not connected.";

    public static int MaximumUserCount { get; } = 3;

    public static bool IsConnected(uint userIndex)
    {
        if (userIndex > MaximumUserCount)
            throw new ArgumentOutOfRangeException(nameof(userIndex));

        var state = new XINPUT_STATE();

        var getState = XInputGetState(userIndex, ref state);

        return getState switch
        {
            ERROR_SUCCESS              => true,
            ERROR_DEVICE_NOT_CONNECTED => false,
            _                          => throw new InvalidOperationException($"{MsgUnknownError}: {getState}.")
        };
    }

    public static XInputControllerState GetState(uint userIndex)
    {
        if (userIndex > MaximumUserCount)
            throw new ArgumentOutOfRangeException(nameof(userIndex));

        var state = new XINPUT_STATE();

        var getState = XInputGetState(userIndex, ref state);

        return getState switch
        {
            ERROR_SUCCESS              => new XInputControllerState(state),
            ERROR_DEVICE_NOT_CONNECTED => throw new InvalidOperationException(MsgDeviceNotConnected),
            _                          => throw new InvalidOperationException($"{MsgUnknownError}: {getState}.")
        };
    }

    public static void SetState(uint userIndex, ushort loRumble, ushort hiRumble)
    {
        if (userIndex > MaximumUserCount)
            throw new ArgumentOutOfRangeException(nameof(userIndex));

        var vibration = new XINPUT_VIBRATION();

        var setState = XInputSetState(userIndex, ref vibration);

        switch (setState)
        {
            case ERROR_SUCCESS: break;
            case ERROR_DEVICE_NOT_CONNECTED: throw new InvalidOperationException(MsgDeviceNotConnected);
            default: throw new InvalidOperationException($"{MsgUnknownError}: {setState}.");
        }
    }
}