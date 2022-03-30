using Microsoft.Extensions.Logging;

namespace PSX.Logging;

public sealed class ObservableLoggerProvider : ILoggerProvider, IObservableLog
{
    public ObservableLoggerProvider()
    {
        Entries = new ObservableQueue<string>(20);
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ObservableLogger(categoryName, Entries);
    }

    public ObservableQueue<string> Entries { get; }
}