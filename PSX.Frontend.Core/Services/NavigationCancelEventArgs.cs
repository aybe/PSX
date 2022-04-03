using System.ComponentModel;

namespace PSX.Frontend.Core.Services;

public sealed class NavigationCancelEventArgs : CancelEventArgs
{
    public NavigationCancelEventArgs(object target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public object Target { get; }

    public override string ToString()
    {
        return $"{nameof(Target)}: {Target}";
    }
}