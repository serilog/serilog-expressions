using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Wildcards
{
    class WildcardComprehensionTransformer : IdentityTransformer
    {
        int _nextParameter;

        public static Expression Expand(Expression root)
        {
            var wc = new WildcardComprehensionTransformer();
            return wc.Transform(root);
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (!Operators.WildcardComparators.Contains(lx.OperatorName))
                return base.Transform(lx);

            IndexerExpression? indexer = null;
            Expression? wildcardPath = null;
            var indexerOperand = -1;
            for (var i = 0; i < lx.Operands.Length; ++i)
            {
                indexer = WildcardSearch.FindElementAtWildcard(lx.Operands[i]);
                if (indexer != null)
                {
                    indexerOperand = i;
                    wildcardPath = lx.Operands[i];
                    break;
                }
            }
            
            if (indexer == null || wildcardPath == null)
                return base.Transform(lx); // N/A, or invalid

            var px = new ParameterExpression("p" + _nextParameter++);
            var nestedComparand = NodeReplacer.Replace(wildcardPath, indexer, px);

            var coll = indexer.Receiver;
            var wc = ((IndexerWildcardExpression)indexer.Index).Wildcard;

            var comparisonArgs = lx.Operands.ToArray();
            comparisonArgs[indexerOperand] = nestedComparand;
            var body = new CallExpression(lx.IgnoreCase, lx.OperatorName, comparisonArgs);
            
            var lambda = new LambdaExpression(new[] { px }, body);

            var op = Operators.ToRuntimeWildcardOperator(wc);
            var call = new CallExpression(false, op, coll, lambda);
            return Transform(call);
        }
    }
}
