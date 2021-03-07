using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Wildcards
{
    class WildcardSearch : SerilogExpressionTransformer<IndexerExpression?>
    {
        static readonly WildcardSearch Instance = new WildcardSearch();
        
        public static IndexerExpression? FindWildcardIndexer(Expression fx)
        {
            return Instance.Transform(fx);
        }

        protected override IndexerExpression? Transform(IndexerExpression ix)
        {
            if (ix.Index is IndexerWildcardExpression)
                return ix;

            return Transform(ix.Receiver);
        }

        protected override IndexerExpression? Transform(ConstantExpression cx)
        {
            return null;
        }
        
        protected override IndexerExpression? Transform(AmbientPropertyExpression px)
        {
            return null;
        }

        protected override IndexerExpression? Transform(AccessorExpression spx)
        {
            return Transform(spx.Receiver);
        }

        protected override IndexerExpression? Transform(LambdaExpression lmx)
        {
            return null;
        }

        protected override IndexerExpression? Transform(ParameterExpression prx)
        {
            return null;
        }

        protected override IndexerExpression? Transform(IndexerWildcardExpression wx)
        {
            // Must be within an indexer
            return null;
        }

        protected override IndexerExpression? Transform(ArrayExpression ax)
        {
            return null;
        }

        protected override IndexerExpression? Transform(CallExpression lx)
        {
            // If we hit a wildcard-compatible operation, then any wildcards within its operands "belong" to
            // it and can't be the result of this search.
            if (Operators.WildcardComparators.Contains(lx.OperatorName))
                return null;
    
            return lx.Operands.Select(Transform).FirstOrDefault(e => e != null);
        }

        protected override IndexerExpression? Transform(IndexOfMatchExpression mx)
        {
            return Transform(mx.Corpus);
        }

        protected override IndexerExpression? Transform(ObjectExpression ox)
        {
            return null;
        }
    }
}
