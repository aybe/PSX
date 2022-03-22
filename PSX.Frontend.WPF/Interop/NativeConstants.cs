using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.WPF.Interop;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal static partial class NativeConstants
{
    public const nint INVALID_HANDLE_VALUE = -1;

    public const uint STD_OUTPUT_HANDLE = unchecked((uint)-11);

    public const uint SWP_NOSIZE = 1;

    public const int MF_BYCOMMAND = 0x00000000;

    public const int SC_CLOSE = 0xF060;

    public const int HC_ACTION = 0;

    public const int WH_KEYBOARD_LL = 13;

    public const int WM_KEYDOWN = 0x0100;

    public const int WM_KEYUP = 0x0101;

    public const int WM_SYSKEYDOWN = 0x0104;

    public const int WM_SYSKEYUP = 0x0105;

    public const int VK_F4 = 0x73;

    public const int KF_ALTDOWN = 0x2000;

    public const int LLKHF_ALTDOWN = KF_ALTDOWN >> 8;
}