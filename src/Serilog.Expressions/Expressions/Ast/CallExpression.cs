using System;
using System.Linq;

namespace Serilog.Expressions.Ast
{
    class CallExpression : Expression
    {
        public CallExpression(bool ignoreCase, string operatorName, params Expression[] operands)
        {
            IgnoreCase = ignoreCase;
            OperatorName = operatorName ?? throw new ArgumentNullException(nameof(operatorName));
            Operands = operands ?? throw new ArgumentNullException(nameof(operands));
        }

        public bool IgnoreCase { get; }
        
        public string OperatorName { get; }

        public Expression[] Operands { get; }

        public override string ToString()
        {
            return OperatorName
                   + "(" + string.Join(", ", Operands.Select(o => o.ToString())) + ")"
                   + (IgnoreCase ? " ci" : "");
        }
    }
}
