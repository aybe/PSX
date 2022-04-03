namespace PSX.Frontend.Services.Navigation;

public interface INavigationTarget
{
    public object? DataContext { get; set; }

    bool IsVisible { get; }

    bool Activate();

    void Show();

    void Hide();
}