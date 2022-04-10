using PSX.Devices.Input;
using static PSX.Devices.Input.InputAction;
using static PSX.Frontend.Input.XInput.Interop.NativeConstants;
using static PSX.Frontend.Input.XInput.XInputControllerState;

namespace PSX.Frontend.Input.XInput;

public sealed class XInputControllerSource : BaseControllerSource
{
    public XInputControllerSource(uint userIndex)
    {
        if (userIndex > XInputController.MaximumUserCount)
            throw new ArgumentOutOfRangeException(nameof(userIndex), userIndex, null);

        UserIndex = userIndex;
    }

    private uint UserIndex { get; }

    private IDictionary<InputAction, float> Dictionary { get; } = new Dictionary<InputAction, float>
    {
        { DPadUp, default },
        { DPadDown, default },
        { DPadLeft, default },
        { DPadRight, default },
        { Triangle, default },
        { Circle, default },
        { Cross, default },
        { Square, default },
        { Select, default },
        { Start, default },
        { L1, default },
        { L2, default },
        { L3, default },
        { R1, default },
        { R2, default },
        { R3, default },
        { LeftX, default },
        { LeftY, default },
        { RightX, default },
        { RightY, default }
    };

    public override float GetValue(InputAction inputAction)
    {
        return Dictionary[inputAction];
    }

    public override void Update()
    {
        if (!XInputController.IsConnected(UserIndex))
            return;

        var state = XInputController.GetState(UserIndex);

        Dictionary[DPadUp]    = Convert.ToSingle(state.DPadUp);
        Dictionary[DPadDown]  = Convert.ToSingle(state.DPadDown);
        Dictionary[DPadLeft]  = Convert.ToSingle(state.DPadLeft);
        Dictionary[DPadRight] = Convert.ToSingle(state.DPadRight);
        Dictionary[Triangle]  = Convert.ToSingle(state.Y);
        Dictionary[Circle]    = Convert.ToSingle(state.B);
        Dictionary[Cross]     = Convert.ToSingle(state.A);
        Dictionary[Square]    = Convert.ToSingle(state.X);
        Dictionary[Select]    = Convert.ToSingle(state.Back);
        Dictionary[Start]     = Convert.ToSingle(state.Start);
        Dictionary[L1]        = Convert.ToSingle(state.LeftShoulder);
        Dictionary[R1]        = Convert.ToSingle(state.RightShoulder);
        Dictionary[L2]        = FilterTrigger(state.LeftTrigger,  XINPUT_GAMEPAD_TRIGGER_THRESHOLD) > 0 ? 1 : 0;
        Dictionary[R2]        = FilterTrigger(state.RightTrigger, XINPUT_GAMEPAD_TRIGGER_THRESHOLD) > 0 ? 1 : 0;
        Dictionary[L3]        = Convert.ToSingle(state.LeftThumb);
        Dictionary[R3]        = Convert.ToSingle(state.RightThumb);

        var ls = FilterStick(state.LeftThumbStickX,  state.LeftThumbStickY,  XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE);
        var rs = FilterStick(state.RightThumbStickX, state.RightThumbStickY, XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE);

        const short sMin = short.MinValue;
        const short sMax = short.MaxValue;
        const byte  tMin = byte.MinValue;
        const byte  tMax = byte.MaxValue;

        Dictionary[LeftX] = Normalize(ls.X, sMin, sMax, tMin, tMax);
        Dictionary[LeftY] = Normalize(ls.Y, sMin, sMax, tMin, tMax);

        Dictionary[RightX] = Normalize(rs.X, sMin, sMax, tMin, tMax);
        Dictionary[RightY] = Normalize(rs.Y, sMin, sMax, tMin, tMax);
    }
}