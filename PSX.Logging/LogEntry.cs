using Microsoft.Extensions.Logging;

namespace PSX.Logging;

public readonly struct LogEntry
{
    public LogLevel LogLevel { get; }

    public string Category { get; }

    public EventId EventId { get; }

    public IReadOnlyList<KeyValuePair<string, object?>> State { get; }

    public Exception? Exception { get; }

    public Func<IReadOnlyList<KeyValuePair<string, object?>>, Exception?, string>? Formatter { get; }

    public LogEntry(
        LogLevel logLevel,
        string category,
        EventId eventId,
        IReadOnlyList<KeyValuePair<string, object?>> state,
        Exception? exception,
        Func<IReadOnlyList<KeyValuePair<string, object?>>, Exception?, string> formatter
    )
    {
        LogLevel  = logLevel;
        Category  = category;
        EventId   = eventId;
        State     = state;
        Exception = exception;
        Formatter = formatter;
    }

    public override string ToString()
    {
        return $"{nameof(LogLevel)}: {LogLevel}, {nameof(State)}: {State}";
    }
}