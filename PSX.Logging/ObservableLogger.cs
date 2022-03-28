using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PSX.Logging;

internal sealed class ObservableLogger : ILogger
{
    public ObservableLogger(string categoryName, ObservableLoggerCollection<LogEntry> collection)
    {
        CategoryName = categoryName;
        Entries      = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    private string CategoryName { get; }

    private SynchronizationContext? Context { get; } = SynchronizationContext.Current;

    private List<LogEntry> EntriesPrivate { get; } = new();

    private ObservableLoggerCollection<LogEntry> Entries { get; }

    private Stopwatch Stopwatch { get; } = new();

    private TimeSpan Duration { get; } = TimeSpan.FromSeconds(1.0d); // TODO this should be configurable from outside

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
            Formatter // TODO how does that perform in practice? // TODO BUG profiling sucks on that one
        );

        EntriesPrivate.Add(entry);

        if (Stopwatch.IsRunning)
        {
            if (Stopwatch.Elapsed >= Duration)
            {
                if (SynchronizationContext.Current == Context)
                {
                    SendOrPostCallback(EntriesPrivate);
                }
                else
                {
                    Context?.Send(SendOrPostCallback, EntriesPrivate);
                }

                Stopwatch.Restart();
            }
        }
        else
        {
            Stopwatch.Restart();
        }

        void SendOrPostCallback(object? o)
        {
            if (o is not IEnumerable<LogEntry> entries)
                throw new InvalidOperationException();

            Entries.AddRange(entries);
            EntriesPrivate.Clear();
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new NullScope();
    }

    private readonly struct NullScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}