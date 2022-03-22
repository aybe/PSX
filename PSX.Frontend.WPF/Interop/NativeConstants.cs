using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.WPF.Interop;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
internal static partial class NativeConstants
{
    public const nint INVALID_HANDLE_VALUE = -1;

    public const uint STD_OUTPUT_HANDLE = unchecked((uint)-11);

    public const uint SWP_NOSIZE = 1;
}