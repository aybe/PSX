using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.Input.XInput.Interop;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
internal static class NativeConstants
{
    public const int ERROR_DEVICE_NOT_CONNECTED          = 1167;
    public const int ERROR_SUCCESS                       = 0;
    public const int XINPUT_GAMEPAD_DPAD_UP              = 0x0001;
    public const int XINPUT_GAMEPAD_DPAD_DOWN            = 0x0002;
    public const int XINPUT_GAMEPAD_DPAD_LEFT            = 0x0004;
    public const int XINPUT_GAMEPAD_DPAD_RIGHT           = 0x0008;
    public const int XINPUT_GAMEPAD_START                = 0x0010;
    public const int XINPUT_GAMEPAD_BACK                 = 0x0020;
    public const int XINPUT_GAMEPAD_LEFT_THUMB           = 0x0040;
    public const int XINPUT_GAMEPAD_RIGHT_THUMB          = 0x0080;
    public const int XINPUT_GAMEPAD_LEFT_SHOULDER        = 0x0100;
    public const int XINPUT_GAMEPAD_RIGHT_SHOULDER       = 0x0200;
    public const int XINPUT_GAMEPAD_A                    = 0x1000;
    public const int XINPUT_GAMEPAD_B                    = 0x2000;
    public const int XINPUT_GAMEPAD_X                    = 0x4000;
    public const int XINPUT_GAMEPAD_Y                    = 0x8000;
    public const int XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE  = 7849;
    public const int XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE = 8689;
    public const int XINPUT_GAMEPAD_TRIGGER_THRESHOLD    = 30;
}