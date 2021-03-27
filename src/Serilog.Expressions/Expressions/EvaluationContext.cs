using System;
using Serilog.Events;
using Serilog.Expressions.Runtime;

namespace Serilog.Expressions
{
    readonly struct EvaluationContext
    {
        public LogEvent LogEvent { get; }
        public Locals? Locals { get; }

        public EvaluationContext(LogEvent logEvent, Locals? locals = null)
        {
            LogEvent = logEvent ?? throw new ArgumentNullException(nameof(logEvent));
            Locals = locals;
        }
    }
}
