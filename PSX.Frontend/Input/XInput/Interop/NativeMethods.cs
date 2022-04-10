using System.Runtime.InteropServices;

namespace PSX.Frontend.Input.XInput.Interop;

internal static class NativeMethods
{
    private const string DllName = "Xinput1_4.dll";

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetstate
    /// </summary>
    [DllImport(DllName)]
    public static extern uint XInputGetState(
        [In]           uint         dwUserIndex,
        [In] [Out] ref XINPUT_STATE pState
    );

    /// <summary>
    ///     https://docs.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputsetstate
    /// </summary>
    [DllImport(DllName)]
    public static extern uint XInputSetState(
        [In]           uint             dwUserIndex,
        [In] [Out] ref XINPUT_VIBRATION pVibration
    );
}