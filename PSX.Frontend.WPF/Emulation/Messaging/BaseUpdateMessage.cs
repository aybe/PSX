using System;

namespace PSX.Frontend.WPF.Emulation.Messaging;

internal sealed class BaseUpdateMessage<T> where T : Delegate
{
    public BaseUpdateMessage(T handler, BaseUpdateMessageType type)
    {
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        Type    = type;
    }

    public T Handler { get; }

    public BaseUpdateMessageType Type { get; }
}