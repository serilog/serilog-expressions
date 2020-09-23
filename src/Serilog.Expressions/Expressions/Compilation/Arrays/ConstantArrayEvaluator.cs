using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Arrays
{
    class ConstantArrayEvaluator : IdentityTransformer
    {
        static readonly ConstantArrayEvaluator Instance = new ConstantArrayEvaluator();
        
        public static Expression Evaluate(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(ArrayExpression ax)
        {
            // This should probably go depth-first.

            if (ax.Elements.All(el => el is ItemElement item &&
                                      item.Value is ConstantExpression))
            {
                return new ConstantExpression(
                    new SequenceValue(ax.Elements
                        .Cast<ItemElement>()
                        .Select(item => ((ConstantExpression)item.Value).Constant)));
            }

            return base.Transform(ax);
        }
    }
}
