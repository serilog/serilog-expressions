using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Parsing;
using Serilog.Templates.Rendering;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    class CompiledFormattedExpression : CompiledTemplate
    {
        readonly ThemedJsonValueFormatter _jsonFormatter;
        readonly Evaluatable _expression;
        readonly string? _format;
        readonly Alignment? _alignment;
        readonly Style _secondaryText;

        public CompiledFormattedExpression(Evaluatable expression, string? format, Alignment? alignment, TemplateTheme theme)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _format = format;
            _alignment = alignment;
            _secondaryText = theme.GetStyle(TemplateThemeStyle.SecondaryText);
            _jsonFormatter = new ThemedJsonValueFormatter(theme);
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider)
        {
            var invisibleCharacterCount = 0;
            
            if (_alignment == null)
            {
                EvaluateUnaligned(ctx, output, formatProvider, ref invisibleCharacterCount);
            }
            else
            {
                var writer = new StringWriter();
                EvaluateUnaligned(ctx, writer, formatProvider, ref invisibleCharacterCount);
                Padding.Apply(output, writer.ToString(), _alignment.Value.Widen(invisibleCharacterCount));
            }
        }
        
        void EvaluateUnaligned(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider, ref int invisibleCharacterCount)
        {
            var value = _expression(ctx);
            if (value == null)
                return; // Undefined is empty
            
            if (value is ScalarValue scalar)
            {
                if (scalar.Value is null)
                    return; // Null is empty

                using var style = _secondaryText.Set(output, ref invisibleCharacterCount);
                
                if (scalar.Value is IFormattable fmt)
                    output.Write(fmt.ToString(_format, formatProvider));
                else
                    output.Write(scalar.Value.ToString());
            }
            else
            {
                invisibleCharacterCount += _jsonFormatter.Format(value, output);
            }
        }
    }
}
