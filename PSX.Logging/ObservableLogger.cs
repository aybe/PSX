using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace PSX.Logging;

internal sealed class ObservableLogger : ILogger
{
    public ObservableLogger(string categoryName, ObservableCollection<LogEntry> collection)
    {
        CategoryName = categoryName;

        Entries = collection;
    }

    public ObservableCollection<LogEntry> Entries { get; }

    private string CategoryName { get; }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        // TODO see what's best VS ConsoleLogger, DebugLogger formatting messages (but here we have LogEntry so...)

        string Formatter(IReadOnlyList<KeyValuePair<string, object?>> i, Exception? e)
        {
            var result = formatter((TState)i, e);

            return result;
        }

        // and there we have our own untyped log entry like Enterprise Library

        var pairs = state as IReadOnlyList<KeyValuePair<string, object?>> ?? throw new InvalidCastException();

        var entry = new LogEntry(
            logLevel,
            CategoryName,
            eventId,
            pairs,
            exception,
            Formatter // TODO how does that perform in practice?
        );

        Entries.Add(entry);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new NullScope();
    }

    private class NullScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}