using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TextDialogResult
{
    None,
    OK,
    Yes,
    No,
    Cancel
}