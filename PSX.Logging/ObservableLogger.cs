//using System.Diagnostics;

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PSX.Logging;

internal sealed class ObservableLogger : ILogger
{
    public ObservableLogger(string categoryName, ObservableQueue<string> collection)
    {
        CategoryName   = categoryName;
        Entries        = collection ?? throw new ArgumentNullException(nameof(collection));
        EntriesPrivate = new Queue<string>(Entries.Capacity);
    }

    public Queue<string> EntriesPrivate { get; }

    private string CategoryName { get; }

    private SynchronizationContext? Context { get; } = SynchronizationContext.Current;

    //private List<string> EntriesPrivate { get; } = new();

    private ObservableQueue<string> Entries { get; }

    private Stopwatch Stopwatch { get; } = new();

    private TimeSpan Span { get; } = TimeSpan.FromSeconds(1.0d / 60.0d); // TODO this should be configurable from outside

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        var count = EntriesPrivate.Count - Entries.Capacity;

        for (var i = 0; i < count; i++)
        {
            EntriesPrivate.Dequeue();
        }

        var message = formatter(state, exception);

        EntriesPrivate.Enqueue(message); // enqueue after so as to stay O(1) in case we're tight

        if (Stopwatch.IsRunning)
        {
            if (Stopwatch.Elapsed >= Span)
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
            if (o is not IEnumerable<string> entries)
                throw new InvalidOperationException();
            
            Entries.Clear();// BUG this sucks
            Entries.EnqueueRange(entries);
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