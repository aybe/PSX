using JetBrains.Annotations;
using Serilog.Expressions;

namespace PSX.Logging.Obsolete;

public abstract class LoggingFilter
    // https://nblumhardt.com/2017/10/logging-filter-switch/
{
    public bool Enabled { get; [PublicAPI] set; } = true;

    protected abstract string ExpressionString { get; }

    protected internal abstract CompiledExpression Expression { get; }

    public override string ToString()
    {
        return ExpressionString;
    }
}