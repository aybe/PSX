using System;
using Un4seen.Bass;

namespace PSX.Frontend.WPF.Sound;

internal sealed class BassException : Exception
{
    public BassException(string? message = null)
    {
        Error = Bass.BASS_ErrorGetCode();

        Message = message is null ? string.Empty : $"{message}: {Error}";
    }

    public BASSError Error { get; }

    public override string Message { get; }
}