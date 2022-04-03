using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace PSX.Frontend.Services.Navigation;

internal sealed class NavigationService : INavigationService
{
    public NavigationService(IServiceProvider serviceProvider, IApplicationService applicationService)
    {
        ServiceProvider    = serviceProvider;
        ApplicationService = applicationService;
    }

    private IServiceProvider ServiceProvider { get; }

    private IApplicationService ApplicationService { get; }

    public void Navigate<TView>() where TView : class
    {
        Navigate<TView>(default);
    }

    public void Navigate<TView, TViewModel>() where TView : class where TViewModel : class
    {
        if (ServiceProvider.GetService<TViewModel>() is not { } viewModel)
        {
            OnNavigationFailed(new NavigationFailedEventArgs($"The view model could not be found: {typeof(TViewModel)}."));
            return;
        }

        Navigate<TView>(viewModel);
    }

    public bool TryGetView<TView>([MaybeNullWhen(false)] out TView result) where TView : class
    {
        return ApplicationService.TryGetView(out result);
    }

    public event NavigationEventHandler? Navigated;

    public event NavigationCancelEventHandler? Navigating;

    public event NavigationFailedEventHandler? NavigationFailed;

    private void Navigate<TView>(object? viewModel) where TView : class
    {
        if (TryGetView<TView>(out var view) is false)
        {
            view = ServiceProvider.GetService<TView>();
        }

        if (view is null)
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

        if (viewModel is not null) // don't change what may have already been injected
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
        if (NavigationFailed is not null)
        {
            NavigationFailed.Invoke(this, e);
        }
        else // throw in this case, that will prevent lots of headaches
        {
            throw new NavigationFailedException(e.Message, e.Exception);
        }
    }

    private void OnNavigated(NavigationEventArgs e)
    {
        Navigated?.Invoke(this, e);
    }
}