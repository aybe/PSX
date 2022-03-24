using Serilog.Core;
using Serilog.Expressions;

namespace PSX.Logging;

public sealed class LoggingFilterSourceContext<T> : LoggingFilter
{
    public LoggingFilterSourceContext()
    {
        ExpressionString = $"{Constants.SourceContextPropertyName} ='{typeof(T).FullName}'";
        Expression       = SerilogExpression.Compile(ExpressionString);
    }

    protected override string ExpressionString { get; }

    protected internal override CompiledExpression Expression { get; }
}