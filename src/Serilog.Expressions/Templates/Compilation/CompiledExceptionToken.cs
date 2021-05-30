using System.IO;
using Serilog.Expressions;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    class CompiledExceptionToken : CompiledTemplate
    {
        const string StackFrameLinePrefix = "   ";

        readonly Style _text, _secondaryText;
        
        public CompiledExceptionToken(TemplateTheme theme)
        {
            _text = theme.GetStyle(TemplateThemeStyle.Text);
            _secondaryText = theme.GetStyle(TemplateThemeStyle.SecondaryText);
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output)
        {
            // Padding and alignment are not applied by this renderer.

            if (ctx.LogEvent.Exception is null)
                return;

            var lines = new StringReader(ctx.LogEvent.Exception.ToString());
            string? nextLine;
            while ((nextLine = lines.ReadLine()) != null)
            {
                var style = nextLine.StartsWith(StackFrameLinePrefix) ? _secondaryText : _text;
                var _ = 0;
                using (style.Set(output, ref _))
                    output.WriteLine(nextLine);
            }
        }
    }
}
