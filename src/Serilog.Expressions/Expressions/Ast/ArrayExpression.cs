using System;
using System.Linq;

namespace Serilog.Expressions.Ast
{
    class ArrayExpression : Expression
    {
        public ArrayExpression(Element[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public Element[] Elements { get; }

        public override string ToString()
        {
            return "[" + string.Join(", ", Elements.Select(o => o.ToString())) + "]";
        }
    }
}
