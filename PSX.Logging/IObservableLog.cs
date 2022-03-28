using System.Collections.ObjectModel;

namespace PSX.Logging;

public interface IObservableLog
{
    ObservableCollection<LogEntry> Entries { get; }
}