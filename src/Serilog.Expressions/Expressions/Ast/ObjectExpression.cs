using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Serilog.Expressions.Ast
{
    class ObjectExpression : Expression
    {
        public ObjectExpression(KeyValuePair<string, Expression>[] members)
        {
            Members = members;
        }
     
        public KeyValuePair<string, Expression>[] Members { get; }

        public override string ToString()
        {
            return "{" + string.Join(", ", Members.Select(m => $"{new ScalarValue(m.Key)}: {m.Value}")) + "}";
        }
    }
}
