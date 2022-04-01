using JetBrains.Annotations;

namespace PSX.Frontend.Core.Old.Navigation;

[PublicAPI]
public interface INavigationService
{
    void Navigate<TView>();

    void Navigate<TView, TViewModel>();

    event NavigationEventHandler? Navigated;

    event NavigationCancelEventHandler? Navigating;

    event NavigationFailedEventHandler? NavigationFailed;
}