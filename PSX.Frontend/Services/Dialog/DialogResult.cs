using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.Services.Dialog;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DialogResult
{
    None,
    OK,
    Yes,
    No,
    Cancel
}