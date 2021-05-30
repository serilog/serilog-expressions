using System.IO;
using Serilog.Expressions;
using Serilog.Parsing;
using Serilog.Templates.Rendering;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    class CompiledLevelToken : CompiledTemplate
    {
        readonly string? _format;
        readonly Alignment? _alignment;
        readonly Style[] _levelStyles;

        public CompiledLevelToken(string? format, Alignment? alignment, TemplateTheme theme)
        {
            _format = format;
            _alignment = alignment;
            _levelStyles = new[]
            {
                theme.GetStyle(TemplateThemeStyle.LevelVerbose),
                theme.GetStyle(TemplateThemeStyle.LevelDebug),
                theme.GetStyle(TemplateThemeStyle.LevelInformation),
                theme.GetStyle(TemplateThemeStyle.LevelWarning),
                theme.GetStyle(TemplateThemeStyle.LevelError),
                theme.GetStyle(TemplateThemeStyle.LevelFatal),
            };
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output)
        {
            var invisibleCharacterCount = 0;
            
            if (_alignment == null)
            {
                EvaluateUnaligned(ctx, output, ref invisibleCharacterCount);
            }
            else
            {
                var writer = new StringWriter();
                EvaluateUnaligned(ctx, writer, ref invisibleCharacterCount);
                Padding.Apply(output, writer.ToString(), _alignment.Value.Widen(invisibleCharacterCount));
            }
        }
        
        void EvaluateUnaligned(EvaluationContext ctx, TextWriter output, ref int invisibleCharacterCount)
        {
            var levelIndex = (int) ctx.LogEvent.Level;
            if (levelIndex < 0 || levelIndex >= _levelStyles.Length)
                return;
            
            using var _ = _levelStyles[levelIndex].Set(output, ref invisibleCharacterCount);
            output.Write(LevelRenderer.GetLevelMoniker(ctx.LogEvent.Level, _format));
        }
    }
}
