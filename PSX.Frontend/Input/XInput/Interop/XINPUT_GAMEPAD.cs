using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PSX.Frontend.Input.XInput.Interop;

/// <summary>
///     https://docs.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_gamepad
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
internal struct XINPUT_GAMEPAD
{
    public ushort wButtons;

    public byte bLeftTrigger;

    public byte bRightTrigger;

    public short sThumbLX;

    public short sThumbLY;

    public short sThumbRX;

    public short sThumbRY;
}