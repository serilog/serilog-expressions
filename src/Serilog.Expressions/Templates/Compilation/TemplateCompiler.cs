using System;
using System.Linq;
using Serilog.Expressions.Compilation;
using Serilog.Templates.Ast;

namespace Serilog.Templates.Compilation
{
    static class TemplateCompiler
    {
        public static CompiledTemplate Compile(Template template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return template switch
            {
                LiteralText text => new CompiledLiteralText(text.Text),
                FormattedExpression expression => new CompiledFormattedExpression(
                    ExpressionCompiler.Compile(expression.Expression), expression.Format),
                TemplateBlock block => new CompiledTemplateBlock(block.Elements.Select(Compile).ToArray()),
                _ => throw new NotSupportedException()
            };
        }
    }
}