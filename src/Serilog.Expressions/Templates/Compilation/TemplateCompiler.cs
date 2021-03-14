using System;
using System.Linq;
using Serilog.Expressions;
using Serilog.Expressions.Compilation;
using Serilog.Templates.Ast;

namespace Serilog.Templates.Compilation
{
    static class TemplateCompiler
    {
        public static CompiledTemplate Compile(Template template, NameResolver nameResolver)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return template switch
            {
                LiteralText text => new CompiledLiteralText(text.Text),
                FormattedExpression expression => new CompiledFormattedExpression(
                    ExpressionCompiler.Compile(expression.Expression, nameResolver), expression.Format, expression.Alignment),
                TemplateBlock block => new CompiledTemplateBlock(block.Elements.Select(e => Compile(e, nameResolver)).ToArray()),
                Conditional conditional => new CompiledConditional(
                    ExpressionCompiler.Compile(conditional.Condition, nameResolver),
                    Compile(conditional.Consequent, nameResolver),
                    conditional.Alternative == null ? null : Compile(conditional.Consequent, nameResolver)),
                _ => throw new NotSupportedException()
            };
        }
    }
}