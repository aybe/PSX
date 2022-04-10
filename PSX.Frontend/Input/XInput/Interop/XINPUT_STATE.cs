using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PSX.Frontend.Input.XInput.Interop;

/// <summary>
///     https://docs.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_state
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
internal struct XINPUT_STATE
{
    public uint dwPacketNumber;

    public XINPUT_GAMEPAD Gamepad;
}