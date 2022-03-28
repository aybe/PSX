using Serilog.Expressions;

namespace PSX.Logging.Obsolete;

public sealed class LoggingFilterString : LoggingFilter
    // https://github.com/serilog/serilog-expressions#language-reference
{
    public LoggingFilterString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        ExpressionString = $"@mt like '%{value}%'";
        Expression       = SerilogExpression.Compile(ExpressionString);
    }

    protected internal override CompiledExpression Expression { get; }

    protected override string ExpressionString { get; }
}