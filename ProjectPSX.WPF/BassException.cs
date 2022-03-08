using System;
using Un4seen.Bass;

namespace ProjectPSX.WPF;

internal sealed class BassException : Exception
{
    public BassException() : base(Bass.BASS_ErrorGetCode().ToString())
    {
    }
}