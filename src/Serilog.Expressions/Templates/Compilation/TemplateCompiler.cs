using System;
using System.Linq;
using Serilog.Expressions;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation;
using Serilog.Templates.Ast;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    static class TemplateCompiler
    {
        public static CompiledTemplate Compile(Template template, NameResolver nameResolver, TemplateTheme? theme)
        {
            // Currently, the same implementations are used for themed and un-themed output; in future we could optimize
            // here by choosing simpler variants when `theme` is `null`.
            
            return template switch
            {
                LiteralText text => new CompiledLiteralText(text.Text, theme ?? TemplateTheme.None),
                FormattedExpression { Expression: AmbientNameExpression { IsBuiltIn: true, PropertyName: BuiltInProperty.Level} } level => new CompiledLevelToken(
                    level.Format, level.Alignment, theme ?? TemplateTheme.None),
                FormattedExpression
                {
                    Expression: AmbientNameExpression { IsBuiltIn: true, PropertyName: BuiltInProperty.Exception }, 
                    Alignment: null,
                    Format: null
                } => new CompiledExceptionToken(theme ?? TemplateTheme.None),
                FormattedExpression expression => new CompiledFormattedExpression(
                    ExpressionCompiler.Compile(expression.Expression, nameResolver), expression.Format, expression.Alignment, theme ?? TemplateTheme.None),
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