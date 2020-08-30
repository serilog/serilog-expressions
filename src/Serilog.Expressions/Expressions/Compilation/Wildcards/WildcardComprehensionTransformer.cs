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
            if (!Operators.WildcardComparators.Contains(lx.OperatorName) || lx.Operands.Length != 2)
                return base.Transform(lx);

            var lhsIs = WildcardSearch.FindElementAtWildcard(lx.Operands[0]);
            var rhsIs = WildcardSearch.FindElementAtWildcard(lx.Operands[1]);
            if (lhsIs != null && rhsIs != null || lhsIs == null && rhsIs == null)
                return base.Transform(lx); // N/A, or invalid

            var wcpath = lhsIs != null ? lx.Operands[0] : lx.Operands[1];
            var comparand = lhsIs != null ? lx.Operands[1] : lx.Operands[0];
            var elmat = lhsIs ?? rhsIs;

            var parm = new ParameterExpression("p" + _nextParameter++);
            var nestedComparand = NodeReplacer.Replace(wcpath, elmat, parm);

            var coll = elmat.Receiver;
            var wc = ((IndexerWildcardExpression)elmat.Index).Wildcard;

            var comparisonArgs = lhsIs != null ? new[] { nestedComparand, comparand } : new[] { comparand, nestedComparand };
            var body = new CallExpression(lx.OperatorName, comparisonArgs);
            var lambda = new LambdaExpression(new[] { parm }, body);

            var op = Operators.ToRuntimeWildcardOperator(wc);
            var call = new CallExpression(op, coll, lambda);
            return Transform(call);
        }
    }
}
