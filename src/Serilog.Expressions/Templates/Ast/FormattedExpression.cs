using System;
using Serilog.Expressions.Ast;
using Serilog.Parsing;

namespace Serilog.Templates.Ast
{
    class FormattedExpression : Template
    {
        public Expression Expression { get; }
        public string? Format { get; }
        public Alignment? Alignment { get; }

        public FormattedExpression(Expression expression, string? format, Alignment? alignment)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Format = format;
            Alignment = alignment;
        }
    }
}
