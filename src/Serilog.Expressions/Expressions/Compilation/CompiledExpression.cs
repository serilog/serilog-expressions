using Serilog.Events;

namespace Serilog.Expressions.Compilation
{
    delegate object CompiledExpression(LogEvent context);
}
