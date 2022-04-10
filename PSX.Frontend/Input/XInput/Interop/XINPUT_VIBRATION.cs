using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PSX.Frontend.Input.XInput.Interop;

/// <summary>
///     https://docs.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_vibration
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
internal struct XINPUT_VIBRATION
{
    public ushort wLeftMotorSpeed;

    public ushort wRightMotorSpeed;
}