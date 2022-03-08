using System;
using Un4seen.Bass;

namespace ProjectPSX.WPF.Sound;

internal sealed class BassException : Exception
{
    public BassException(string message) : base($"{message}: {Bass.BASS_ErrorGetCode()}")
    {
    }
}