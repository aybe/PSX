namespace PSX.Frontend.Core.Services;

public sealed class NavigationFailedException : Exception
{
    public NavigationFailedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}