using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.Services.Dialog;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DialogButton
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}