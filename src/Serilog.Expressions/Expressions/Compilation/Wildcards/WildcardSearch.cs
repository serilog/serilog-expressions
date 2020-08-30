using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Wildcards
{
    class WildcardSearch : SerilogExpressionTransformer<IndexerExpression>
    {
        public static IndexerExpression FindElementAtWildcard(Expression fx)
        {
            var search = new WildcardSearch();
            return search.Transform(fx);
        }

        protected override IndexerExpression Transform(IndexerExpression ix)
        {
            if (ix.Index is IndexerWildcardExpression)
                return ix;

            return Transform(ix.Receiver);
        }

        protected override IndexerExpression Transform(ConstantExpression cx)
        {
            return null;
        }
        
        protected override IndexerExpression Transform(AmbientPropertyExpression px)
        {
            return null;
        }

        protected override IndexerExpression Transform(AccessorExpression spx)
        {
            return Transform(spx.Receiver);
        }

        protected override IndexerExpression Transform(LambdaExpression lmx)
        {
            return null;
        }

        protected override IndexerExpression Transform(ParameterExpression prx)
        {
            return null;
        }

        protected override IndexerExpression Transform(IndexerWildcardExpression wx)
        {
            // Must be within an indexer
            return null;
        }

        protected override IndexerExpression Transform(ArrayExpression ax)
        {
            return null;
        }

        protected override IndexerExpression Transform(CallExpression lx)
        {
            return lx.Operands.Select(Transform).FirstOrDefault(e => e != null);
        }
    }
}
