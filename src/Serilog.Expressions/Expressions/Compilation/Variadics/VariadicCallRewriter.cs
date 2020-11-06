using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Variadics
{
    class VariadicCallRewriter : IdentityTransformer
    {
        static readonly VariadicCallRewriter Instance = new VariadicCallRewriter();

        public static Expression Rewrite(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (Operators.SameOperator(lx.OperatorName, Operators.OpSubstring) && lx.Operands.Length == 2)
            {
                var operands = lx.Operands
                    .Select(Transform)
                    .Concat(new[] {new CallExpression(false, Operators.OpUndefined)})
                    .ToArray();
                return new CallExpression(lx.IgnoreCase, lx.OperatorName, operands);
            }

            if (Operators.SameOperator(lx.OperatorName, Operators.OpCoalesce))
            {
                if (lx.Operands.Length == 0)
                    return new CallExpression(false, Operators.OpUndefined);
                if (lx.Operands.Length == 1)
                    return Transform(lx.Operands.Single());
                if (lx.Operands.Length > 2)
                {
                    var first = Transform(lx.Operands.First());
                    return new CallExpression(false, lx.OperatorName, first,
                        Transform(new CallExpression(false, lx.OperatorName, lx.Operands.Skip(1).ToArray())));
                }
            }

            return base.Transform(lx);
        }
    }
}