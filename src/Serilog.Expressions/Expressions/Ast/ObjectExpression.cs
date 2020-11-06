using System.Linq;

namespace Serilog.Expressions.Ast
{
    class ObjectExpression : Expression
    {
        public ObjectExpression(Member[] members)
        {
            Members = members;
        }
     
        public Member[] Members { get; }

        public override string ToString()
        {
            return "{" + string.Join(", ", Members.Select(m => m.ToString())) + "}";
        }
    }
}

