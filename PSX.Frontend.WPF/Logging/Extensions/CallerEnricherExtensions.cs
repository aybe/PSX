using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace PSX.Frontend.WPF.Logging.Extensions;

[SuppressMessage("ReSharper", "IdentifierTypo")]
internal static class CallerEnricherExtensions
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return configuration.With<CallerEnricher>();
    }

    private sealed class CallerEnricher : ILogEventEnricher
    {
        private static Assembly SerilogAssembly { get; } = typeof(Log).Assembly;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(GetString() ?? "<unknown method>")));
        }

        private static string? GetString(int methodsToSkip = 2)
        {
            if (methodsToSkip <= 0)
                throw new ArgumentOutOfRangeException(nameof(methodsToSkip));

            var trace = new StackTrace(StackTrace.METHODS_TO_SKIP + methodsToSkip, true);

            for (var i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                if (frame == null)
                    throw new InvalidOperationException($"Couldn't get stack frame at index {i}.");

                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var type = method.DeclaringType;
                if (type == null)
                    continue;

                if (type.Assembly == SerilogAssembly)
                    continue;

                var methodName = type.FullName;
                var methodParams = string.Join(", ", method.GetParameters().Select(s => s.ParameterType.FullName));
                var methodFile = frame.GetFileName();
                var methodLine = frame.GetFileLineNumber();

                return $"at {methodName}.{method.Name}({methodParams}) in {methodFile}:line {methodLine}";
            }

            return null;
        }
    }
}