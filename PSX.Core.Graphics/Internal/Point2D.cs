﻿using System.Runtime.InteropServices;

namespace PSX.Core.Graphics.Internal;

[StructLayout(LayoutKind.Explicit)]
internal struct Point2D
{
    [FieldOffset(0)]
    public short X;

    [FieldOffset(2)]
    public short Y;
}