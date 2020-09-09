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
            var target = lx;

            if (!Operators.WildcardComparators.Contains(target.OperatorName) || target.Operands.Length != 2)
                return base.Transform(lx);

            var lhsIs = WildcardSearch.FindElementAtWildcard(target.Operands[0]);
            var rhsIs = WildcardSearch.FindElementAtWildcard(target.Operands[1]);
            if (lhsIs != null && rhsIs != null || lhsIs == null && rhsIs == null)
                return base.Transform(lx); // N/A, or invalid

            var wildcardPath = lhsIs != null ? target.Operands[0] : target.Operands[1];
            var comparand = lhsIs != null ? target.Operands[1] : target.Operands[0];
            var indexer = lhsIs ?? rhsIs!;

            var px = new ParameterExpression("p" + _nextParameter++);
            var nestedComparand = NodeReplacer.Replace(wildcardPath, indexer, px);

            var coll = indexer.Receiver;
            var wc = ((IndexerWildcardExpression)indexer.Index).Wildcard;

            var comparisonArgs = lhsIs != null ? new[] { nestedComparand, comparand } : new[] { comparand, nestedComparand };
            var body = new CallExpression(target.OperatorName, comparisonArgs);
            
            var lambda = new LambdaExpression(new[] { px }, body);

            var op = Operators.ToRuntimeWildcardOperator(wc);
            var call = new CallExpression(op, coll, lambda);
            return Transform(call);
        }
    }
}
