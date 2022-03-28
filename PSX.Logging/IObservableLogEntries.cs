using System.Collections.ObjectModel;

namespace PSX.Logging;

public interface IObservableLogEntries
{
    ObservableCollection<LogEntry> Entries { get; }
}