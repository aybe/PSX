namespace PSX.Frontend.WinForms.Interop;

internal enum BltMode : uint
{
    STRETCH_ANDSCANS    = 0x01,
    STRETCH_ORSCANS     = 0x02,
    STRETCH_DELETESCANS = 0x03,
    STRETCH_HALFTONE    = 0x04
}