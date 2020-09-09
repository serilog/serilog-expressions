using System;
using Serilog.Expressions.Ast;

namespace Serilog.Templates.Ast
{
    class FormattedExpression : Template
    {
        public Expression Expression { get; }
        public string? Format { get; }

        public FormattedExpression(Expression expression, string? format)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Format = format;
        }
    }
}