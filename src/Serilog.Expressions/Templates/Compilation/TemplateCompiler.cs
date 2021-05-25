using System;
using System.Linq;
using Serilog.Expressions;
using Serilog.Expressions.Compilation;
using Serilog.Templates.Ast;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    static class TemplateCompiler
    {
        public static CompiledTemplate Compile(Template template, NameResolver nameResolver, TemplateTheme theme)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return template switch
            {
                LiteralText text => new CompiledLiteralText(text.Text, theme),
                FormattedExpression expression => new CompiledFormattedExpression(
                    ExpressionCompiler.Compile(expression.Expression, nameResolver), expression.Format, expression.Alignment),
                TemplateBlock block => new CompiledTemplateBlock(block.Elements.Select(e => Compile(e, nameResolver, theme)).ToArray()),
                Conditional conditional => new CompiledConditional(
                    ExpressionCompiler.Compile(conditional.Condition, nameResolver),
                    Compile(conditional.Consequent, nameResolver, theme),
                    conditional.Alternative == null ? null : Compile(conditional.Alternative, nameResolver, theme)),
                Repetition repetition => new CompiledRepetition(
                    ExpressionCompiler.Compile(repetition.Enumerable, nameResolver),
                    repetition.BindingNames.Length > 0 ? repetition.BindingNames[0] : null,
                    repetition.BindingNames.Length > 1 ? repetition.BindingNames[1] : null,
                    Compile(repetition.Body, nameResolver, theme),
                    repetition.Delimiter == null ? null : Compile(repetition.Delimiter, nameResolver, theme),
                    repetition.Alternative == null ? null : Compile(repetition.Alternative, nameResolver, theme)),
                _ => throw new NotSupportedException()
            };
        }
    }
}