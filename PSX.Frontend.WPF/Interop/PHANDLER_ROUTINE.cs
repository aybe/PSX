using System.Diagnostics.CodeAnalysis;

namespace PSX.Frontend.WPF.Interop;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public delegate int PHANDLER_ROUTINE(uint CtrlType);