using System;
using System.IO;
using Serilog.Expressions;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    class CompiledLiteralText : CompiledTemplate
    {
        readonly string _text;
        readonly Style _style;

        public CompiledLiteralText(string text, TemplateTheme theme)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _style = theme.GetStyle(TemplateThemeStyle.TertiaryText);
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output)
        {
            var _ = 0;
            using (_style.Set(output, ref _))
                output.Write(_text);
        }
    }
}
