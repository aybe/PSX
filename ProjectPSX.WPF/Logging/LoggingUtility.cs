using ProjectPSX.WPF.Logging.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ProjectPSX.WPF.Logging;

public static class LoggingUtility
{
    public static ILogger GetDefaultLogger()
    {
        // https://github.com/serilog/serilog/wiki/Formatting-Output

        const string currentOutputTemplate =
            "[{Timestamp:HH:mm:ss.fff}] {Level} {Message:lj} {Caller}{NewLine}";

        var configuration = new LoggerConfiguration();

        var logger = configuration
            .MinimumLevel.Verbose()
            .Enrich.WithCaller()
            .WriteTo.Console(outputTemplate: currentOutputTemplate, theme: SystemConsoleTheme.Colored)
            .CreateLogger();

        return logger;
    }
}