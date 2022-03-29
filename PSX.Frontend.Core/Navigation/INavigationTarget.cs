using JetBrains.Annotations;

namespace PSX.Frontend.Core.Navigation;

[PublicAPI]
public interface INavigationTarget
{
    public object? DataContext { get; set; }

    void Show();

    void Hide();
}