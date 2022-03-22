namespace PSX.Frontend.WPF.Interop;

/// <summary>
///     https://docs.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
/// </summary>
internal delegate int LowLevelKeyboardProc(
    int nCode,
    uint wParam,
    ref KBDLLHOOKSTRUCT lParam
);