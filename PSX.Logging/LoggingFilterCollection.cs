using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;

namespace PSX.Logging;

public sealed class LoggingFilterCollection : LoggingFilterSwitch, ILogEventFilter, IEnumerable<LoggingFilter>
{
    private readonly ConcurrentBag<LoggingFilter> Filters = new();

    public IEnumerator<LoggingFilter> GetEnumerator()
    {
        return Filters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Filters).GetEnumerator();
    }

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public new bool IsEnabled(LogEvent logEvent)
    {
        if (logEvent is null)
            throw new ArgumentNullException(nameof(logEvent));

        foreach (var filter in Filters)
        {
            if (filter.Enabled && filter.Expression(logEvent) is ScalarValue { Value: true })
            {
                return false;
            }
        }

        return true;
    }

    public void Add(LoggingFilter filter)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        Filters.Add(filter);
    }

    public override string ToString()
    {
        return $"{nameof(Filters)}: {Filters.Count}";
    }
}