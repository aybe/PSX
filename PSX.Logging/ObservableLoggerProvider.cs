using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace PSX.Logging;

public sealed class ObservableLoggerProvider : ILoggerProvider, IObservableLog
{
    public ObservableLoggerProvider()
    {
        Entries = new ObservableCollection<LogEntry>();
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ObservableLogger(categoryName, Entries);
    }

    public ObservableCollection<LogEntry> Entries { get; }
}