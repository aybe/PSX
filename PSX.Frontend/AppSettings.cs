using System.Collections.ObjectModel;

namespace PSX.Frontend;

public sealed class AppSettings
{
    public ObservableCollection<string> RecentlyUsed { get; } = new();
}