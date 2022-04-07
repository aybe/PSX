using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TextDialogButton
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}