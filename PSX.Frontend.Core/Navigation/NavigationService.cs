using Microsoft.Extensions.DependencyInjection;

namespace PSX.Frontend.Core.Navigation;

internal sealed class NavigationService : INavigationService
{
    public NavigationService(IServiceProvider services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    private IServiceProvider Services { get; }

    public void Navigate<TView>()
    {
        TryNavigate(TryNavigateImpl<TView>);
    }

    public void Navigate<TView, TViewModel>()
    {
        TryNavigate(TryNavigateImpl<TView, TViewModel>);
    }

    public event NavigationEventHandler? Navigated;

    public event NavigationCancelEventHandler? Navigating;

    public event NavigationFailedEventHandler? NavigationFailed;

    private void TryNavigate(Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var exception = default(Exception?);

        try
        {
            action();
        }
        catch (Exception e)
        {
            exception = e;
        }

        if (exception is not null)
        {
            OnNavigationFailed(new NavigationFailedEventArgs("An exception has occurred while trying to navigate.", exception));
        }
    }

    private void TryNavigateImpl<TView>()
    {
        TryNavigateImpl<TView>(default);
    }

    private void TryNavigateImpl<TView, TViewModel>()
    {
        if (Services.GetService<TViewModel>() is not { } viewModel)
        {
            OnNavigationFailed(new NavigationFailedEventArgs($"The view model could not be found: {typeof(TViewModel)}."));
            return;
        }

        TryNavigateImpl<TView>(viewModel);
    }

    private void TryNavigateImpl<TView>(object? viewModel)
    {
        if (Services.GetService<TView>() is not { } view)
        {
            OnNavigationFailed(new NavigationFailedEventArgs($"The view could not be found: {typeof(TView)}."));
            return;
        }

        if (view is not INavigationTarget target)
        {
            OnNavigationFailed(new NavigationFailedEventArgs($"The view is not an {nameof(INavigationTarget)}: {view}."));
            return;
        }

        var args = new NavigationCancelEventArgs(target);

        OnNavigating(args);

        if (args.Cancel)
        {
            return;
        }

        if (viewModel is not null) // may have already been injected
        {
            target.DataContext = viewModel;
        }

        if (target.IsVisible)
        {
            target.Activate();
        }
        else
        {
            target.Show();
        }

        OnNavigated(new NavigationEventArgs(target));
    }

    private void OnNavigating(NavigationCancelEventArgs e)
    {
        Navigating?.Invoke(this, e);
    }

    private void OnNavigationFailed(NavigationFailedEventArgs e)
    {
        NavigationFailed?.Invoke(this, e);
    }

    private void OnNavigated(NavigationEventArgs e)
    {
        Navigated?.Invoke(this, e);
    }
}