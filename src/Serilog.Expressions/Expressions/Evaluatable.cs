using Serilog.Events;

namespace Serilog.Expressions
{
    delegate LogEventPropertyValue? Evaluatable(EvaluationContext ctx);
}
