namespace PSX.Frontend.Services.Navigation;

public sealed class NavigationFailedException : Exception
{
    public NavigationFailedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}