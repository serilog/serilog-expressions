using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;

namespace Serilog.Templates.Compilation
{
    class CompiledFormattedExpression : CompiledTemplate
    {
        readonly CompiledExpression _expression;
        readonly string _format;

        public CompiledFormattedExpression(CompiledExpression expression, string format)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _format = format;
        }

        public override void Evaluate(LogEvent logEvent, TextWriter output, IFormatProvider formatProvider)
        {
            var value = _expression(logEvent);
            value?.Render(output, _format, formatProvider);
        }
    }
}