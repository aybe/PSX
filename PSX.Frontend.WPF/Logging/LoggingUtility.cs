using ProjectPSX.WPF.Logging.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ProjectPSX.WPF.Logging;

public static class LoggingUtility
{
    public static ILogger GetDefaultLogger(bool includeCallerInformation = false)
    {
        // https://github.com/serilog/serilog/wiki/Formatting-Output

        const string currentOutputTemplate =
            "[{Timestamp:HH:mm:ss.fff}] {Level} {Message:lj} {Caller}{NewLine}";

        var configuration =
            new LoggerConfiguration()
                .MinimumLevel
                .Verbose()
                .WriteTo.Console(outputTemplate: currentOutputTemplate, theme: SystemConsoleTheme.Colored);

        if (includeCallerInformation)
        {
            configuration = configuration.Enrich.WithCaller();
        }

        var logger = configuration.CreateLogger();

        return logger;
    }
}