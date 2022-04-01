namespace PSX.Frontend.Core.Navigation;

public interface INavigationService
{
    void Navigate<TView>()
        where TView : class;

    void Navigate<TView, TViewModel>()
        where TView : class
        where TViewModel : class;

    event NavigationEventHandler? Navigated;

    event NavigationCancelEventHandler? Navigating;

    event NavigationFailedEventHandler? NavigationFailed;
}