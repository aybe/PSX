﻿using System.Runtime.InteropServices;

namespace ProjectPSX.WinForms.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct RgbQuad
{
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
}