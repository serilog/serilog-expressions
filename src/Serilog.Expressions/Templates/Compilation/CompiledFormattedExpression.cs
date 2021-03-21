using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using Serilog.Templates.Rendering;

namespace Serilog.Templates.Compilation
{
    class CompiledFormattedExpression : CompiledTemplate
    {
        static readonly JsonValueFormatter JsonFormatter = new JsonValueFormatter("$type");
        
        readonly Evaluatable _expression;
        readonly string? _format;
        readonly Alignment? _alignment;

        public CompiledFormattedExpression(Evaluatable expression, string? format, Alignment? alignment)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _format = format;
            _alignment = alignment;
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider)
        {
            if (_alignment == null)
            {
                EvaluateUnaligned(ctx, output, formatProvider);
            }
            else
            {
                var writer = new StringWriter();
                EvaluateUnaligned(ctx, writer, formatProvider);
                Padding.Apply(output, writer.ToString(), _alignment.Value);
            }
        }
        
        void EvaluateUnaligned(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider)
        {
            var value = _expression(ctx);
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
