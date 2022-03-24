using PSX.Logging.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace PSX.Logging;

public static class LoggingFactory
{
    public static LoggingFilterCollection Filters { get; } = new();

    public static LoggingLevelSwitch LevelSwitch { get; } = new();

    public static void Initialize(bool /*TODO remove?*/ includeCallerInformation = false)
    {
        // https://github.com/serilog/serilog/wiki/Formatting-Output

        const string currentOutputTemplate =
            "[{Timestamp:HH:mm:ss.fff}] {Level} {SourceContext} {Message:lj} {Caller}{NewLine}";

        var configuration =
            new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .Filter.ControlledBy(Filters)
                .WriteTo.Async(s => s.Console(outputTemplate: currentOutputTemplate, theme: SystemConsoleTheme.Colored));

        if (includeCallerInformation)
        {
            configuration = configuration.Enrich.WithCaller();
        }

        Log.Logger = configuration.CreateLogger();
    }
}