namespace PSX.Frontend.Core.Navigation;

public interface INavigationService
{
    void Navigate<TView>();

    void Navigate<TView, TViewModel>();

    event NavigationEventHandler? Navigated;

    event NavigationCancelEventHandler? Navigating;

    event NavigationFailedEventHandler? NavigationFailed;
}