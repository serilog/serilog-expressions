using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;
using Serilog.Expressions.Runtime;

namespace Serilog.Expressions.Compilation.Arrays
{
    class ConstantArrayEvaluator : IdentityTransformer
    {
        public static Expression Evaluate(Expression expression)
        {
            var evaluator = new ConstantArrayEvaluator();
            return evaluator.Transform(expression);
        }

        protected override Expression Transform(ArrayExpression ax)
        {
            // This should probably go depth-first.

            if (ax.Elements.All(el => el is ConstantExpression))
            {
                return new ConstantExpression(
                    new SequenceValue(ax.Elements
                        .Cast<ConstantExpression>()
                        .Select(ce => Representation.Recapture(ce.ConstantValue))));
            }

            return base.Transform(ax);
        }
    }
}
