using System.Linq;
using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Compilation.Transformations
{
    class IdentityTransformer : SerilogExpressionTransformer<Expression>
    {
        protected override Expression Transform(CallExpression lx)
        {
            return new CallExpression(lx.OperatorName, lx.Operands.Select(Transform).ToArray());
        }

        protected override Expression Transform(ConstantExpression cx)
        {
            return cx;
        }

        protected override Expression Transform(AmbientPropertyExpression px)
        {
            return px;
        }

        protected override Expression Transform(AccessorExpression spx)
        {
            return new AccessorExpression(Transform(spx.Receiver), spx.MemberName);
        }

        protected override Expression Transform(LambdaExpression lmx)
        {
            // By default we maintain the parameters available in the body
            return new LambdaExpression(lmx.Parameters, Transform(lmx.Body));
        }

        // Only touches uses of the parameters, not decls
        protected override Expression Transform(ParameterExpression prx)
        {
            return prx;
        }

        protected override Expression Transform(IndexerWildcardExpression wx)
        {
            return wx;
        }

        protected override Expression Transform(ArrayExpression ax)
        {
            return new ArrayExpression(ax.Elements.Select(Transform).ToArray());
        }

        protected override Expression Transform(IndexerExpression ix)
        {
            return new IndexerExpression(Transform(ix.Receiver), Transform(ix.Index));
        }
    }
}
