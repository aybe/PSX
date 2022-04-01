﻿namespace PSX.Frontend.Core.Navigation;

public sealed class NavigationFailedEventArgs : EventArgs
{
    public NavigationFailedEventArgs(string message, Exception? exception = null)
    {
        Message   = message;
        Exception = exception;
    }

    public string Message { get; }

    public Exception? Exception { get; }
}