using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Formatting.Json;
using Serilog.Templates.Rendering;

namespace Serilog.Templates.Compilation
{
    class CompiledFormattedExpression : CompiledTemplate
    {
        static readonly JsonValueFormatter JsonFormatter = new JsonValueFormatter("$type");
        
        readonly CompiledExpression _expression;
        readonly string? _format;

        public CompiledFormattedExpression(CompiledExpression expression, string? format)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _format = format;
        }

        public override void Evaluate(LogEvent logEvent, TextWriter output, IFormatProvider? formatProvider)
        {
            var value = _expression(logEvent);
            if (value == null)
                return; // Undefined is empty
            
            if (value is ScalarValue scalar)
            {
                if (scalar.Value is null)
                    return; // Null is empty

                if (scalar.Value is LogEventLevel level)
                    // This would be better implemented using CompiledLevelToken : CompiledTemplate.
                    output.Write(LevelRenderer.GetLevelMoniker(level, _format));
                else if (scalar.Value is IFormattable fmt)
                    output.Write(fmt.ToString(_format, formatProvider));
                else
                    output.Write(scalar.Value.ToString());
            }
            else
            {
                JsonFormatter.Format(value, output);
            }
        }
    }
}