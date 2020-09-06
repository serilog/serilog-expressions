using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Is
{
    class IsOperatorTransformer : IdentityTransformer
    {
        static readonly IsOperatorTransformer Instance = new IsOperatorTransformer();
        
        public static Expression Rewrite(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (!Operators.SameOperator(lx.OperatorName, Operators.IntermediateOpSqlIs) || lx.Operands.Length != 2)
                return base.Transform(lx);

            if (lx.Operands[1] is ConstantExpression nul)
            {
                return nul.Constant != null ? base.Transform(lx) : new CallExpression(Operators.RuntimeOpIsNull, lx.Operands[0]);
            }

            if (!(lx.Operands[1] is CallExpression not) || not.Operands.Length != 1)
                return base.Transform(lx);

            if (!(not.Operands[0] is ConstantExpression cx) ||
                !(cx.Constant is ScalarValue sv) ||
                sv.Value != null)
                return base.Transform(lx);

            return new CallExpression(Operators.RuntimeOpIsNotNull, lx.Operands[0]);
        }
    }
}
