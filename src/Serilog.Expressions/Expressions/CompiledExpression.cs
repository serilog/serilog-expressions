using Serilog.Events;

#nullable enable

namespace Serilog.Expressions
{
    /// <summary>
    /// A compiled expression evaluated against a <see cref="LogEvent"/>.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <returns>The result of evaluating the expression, represented as a <see cref="LogEventPropertyValue"/>,
    /// or <c langword="null">null</c> if the result is undefined.</returns>
    public delegate LogEventPropertyValue? CompiledExpression(LogEvent logEvent);
}
