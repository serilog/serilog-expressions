using System;
using System.Linq;

namespace Serilog.Expressions.Ast
{ 
    class ArrayExpression : Expression
    {
        public ArrayExpression(Expression[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public Expression[] Elements { get; }

        public override string ToString()
        {
            return "[" + string.Join(",", Elements.Select(o => o.ToString())) + "]";
        }
    }
}
