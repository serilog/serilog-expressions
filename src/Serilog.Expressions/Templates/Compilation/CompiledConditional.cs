using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;

namespace Serilog.Templates.Compilation
{
    class CompiledConditional : CompiledTemplate
    {
        readonly CompiledExpression _condition;
        readonly CompiledTemplate _consequent;
        readonly CompiledTemplate? _alternative;

        public CompiledConditional(CompiledExpression condition, CompiledTemplate consequent, CompiledTemplate? alternative)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
            _alternative = alternative;
        }

        public override void Evaluate(LogEvent logEvent, TextWriter output, IFormatProvider? formatProvider)
        {
            if (ExpressionResult.IsTrue(_condition.Invoke(logEvent)))
                _consequent.Evaluate(logEvent, output, formatProvider);
            else
                _alternative?.Evaluate(logEvent, output, formatProvider);
        }
    }
}
