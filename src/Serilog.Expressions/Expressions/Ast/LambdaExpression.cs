using System.Linq;

namespace Serilog.Expressions.Ast
{
    class LambdaExpression : Expression
    {
        public LambdaExpression(ParameterExpression[] parameters, Expression body)
        {
            Parameters = parameters;
            Body = body;
        }

        public ParameterExpression[] Parameters { get; }

        public Expression Body { get; }

        public override string ToString()
        {
            return "|" + string.Join(", ", Parameters.Select(p => p.ToString())) + "| {" + Body + "}";
        }
    }
}
