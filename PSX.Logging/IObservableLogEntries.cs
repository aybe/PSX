using System.Collections.ObjectModel;

namespace PSX.Logging;

public interface IObservableLogEntries // TODO rename
{
    ObservableCollection<LogEntry> Entries { get; }
}