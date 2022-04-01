namespace PSX.Frontend.Core.Old.Navigation;

public sealed class NavigationEventArgs : EventArgs
{
    public NavigationEventArgs(object target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public object Target { get; }


    public override string ToString()
    {
        return $"{nameof(Target)}: {Target}";
    }
}