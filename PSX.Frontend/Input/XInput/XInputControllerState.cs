using System.Numerics;
using PSX.Frontend.Input.XInput.Interop;
using static PSX.Frontend.Input.XInput.Interop.NativeConstants;

namespace PSX.Frontend.Input.XInput;

public readonly struct XInputControllerState
{
    internal XInputControllerState(XINPUT_STATE state)
    {
        PacketNumber = state.dwPacketNumber;

        var gamepad = state.Gamepad;
        var buttons = gamepad.wButtons;

        DPadUp           = default != (buttons & XINPUT_GAMEPAD_DPAD_UP);
        DPadDown         = default != (buttons & XINPUT_GAMEPAD_DPAD_DOWN);
        DPadLeft         = default != (buttons & XINPUT_GAMEPAD_DPAD_LEFT);
        DPadRight        = default != (buttons & XINPUT_GAMEPAD_DPAD_RIGHT);
        Start            = default != (buttons & XINPUT_GAMEPAD_START);
        Back             = default != (buttons & XINPUT_GAMEPAD_BACK);
        A                = default != (buttons & XINPUT_GAMEPAD_A);
        B                = default != (buttons & XINPUT_GAMEPAD_B);
        X                = default != (buttons & XINPUT_GAMEPAD_X);
        Y                = default != (buttons & XINPUT_GAMEPAD_Y);
        LeftShoulder     = default != (buttons & XINPUT_GAMEPAD_LEFT_SHOULDER);
        LeftThumb        = default != (buttons & XINPUT_GAMEPAD_LEFT_THUMB);
        LeftThumbStickX  = gamepad.sThumbLX;
        LeftThumbStickY  = gamepad.sThumbLY;
        LeftTrigger      = gamepad.bLeftTrigger;
        RightThumb       = default != (buttons & XINPUT_GAMEPAD_RIGHT_THUMB);
        RightThumbStickX = gamepad.sThumbRX;
        RightThumbStickY = gamepad.sThumbRY;
        RightTrigger     = gamepad.bRightTrigger;
        RightShoulder    = default != (buttons & XINPUT_GAMEPAD_RIGHT_SHOULDER);
    }

    public static Vector2 FilterStick(short x, short y, short deadZone, float easing = 1.0f)
    {
        var u = Normalize(x, short.MinValue, short.MaxValue, -1.0f, +1.0f);
        var v = Normalize(y, short.MinValue, short.MaxValue, -1.0f, +1.0f);
        var z = (float)deadZone / short.MaxValue;

        var vector = new Vector2(u, v);

        var magnitude = vector.LengthSquared();

        if (magnitude < z)
        {
            return Vector2.Zero;
        }

        if (magnitude > 1.0f)
        {
            magnitude = 1.0f;
        }

        vector *= (magnitude - z) / (1.0f - z);

        u = MathF.Pow(vector.X, easing);
        v = MathF.Pow(vector.Y, easing);

        return new Vector2(u, v);
    }

    public static byte FilterTrigger(byte value, byte deadZone)
    {
        return value >= deadZone ? value : default;
    }

    public static float Normalize(float value, float srcMin, float srcMax, float tgtMin, float tgtMax)
    {
        return tgtMin + (value - srcMin) * (tgtMax - tgtMin) / (srcMax - srcMin);
    }

    public uint PacketNumber { get; }

    public bool DPadUp { get; }

    public bool DPadDown { get; }

    public bool DPadLeft { get; }

    public bool DPadRight { get; }

    public bool Start { get; }

    public bool Back { get; }

    public bool A { get; }

    public bool B { get; }

    public bool X { get; }

    public bool Y { get; }

    public bool LeftShoulder { get; }

    public bool LeftThumb { get; }

    public short LeftThumbStickX { get; }

    public short LeftThumbStickY { get; }

    public byte LeftTrigger { get; }

    public bool RightShoulder { get; }

    public bool RightThumb { get; }

    public short RightThumbStickX { get; }

    public short RightThumbStickY { get; }

    public byte RightTrigger { get; }

    public override string ToString()
    {
        return $"{nameof(PacketNumber)}: {PacketNumber}";
    }
}