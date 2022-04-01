namespace PSX.Frontend.Core.Navigation;

public interface INavigationService
{
    void Navigate<TView>()
        where TView : class;

    void Navigate<TView, TViewModel>()
        where TView : class
        where TViewModel : class;

    bool TryGetView<TView>(out TView? result) where TView : class;

    event NavigationEventHandler? Navigated;

    event NavigationCancelEventHandler? Navigating;

    event NavigationFailedEventHandler? NavigationFailed;
}