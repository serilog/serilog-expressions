using System;
using Serilog.Expressions.Ast;

namespace Serilog.Templates.Ast
{
    class Conditional : Template
    {
        public Expression Condition { get; }
        public Template Consequent { get; }
        public Template? Alternative { get; }

        public Conditional(Expression condition, Template consequent, Template? alternative)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
            Alternative = alternative;
        }
    }
}
