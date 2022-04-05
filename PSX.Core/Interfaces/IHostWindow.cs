﻿namespace PSX.Core.Interfaces;

public interface IHostWindow
{
    void SetVRAMStart(ushort displayVRamStartX, ushort displayVRamStartY);

    void SetDisplayMode(int horizontalRes, int verticalRes, bool is24BitDepth);

    void SetHorizontalRange(ushort displayX1, ushort displayX2);

    void SetVerticalRange(ushort displayY1, ushort displayY2);

    void Render(int[] buffer24, ushort[] buffer16);

    void Play(byte[] samples);
}