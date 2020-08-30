using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Compilation.Transformations
{
    class NodeReplacer : IdentityTransformer
    {
        readonly Expression _source;
        readonly Expression _dest;

        public static Expression Replace(Expression expr, Expression source, Expression dest)
        {
            var replacer = new NodeReplacer(source, dest);
            return replacer.Transform(expr);
        }

        NodeReplacer(Expression source, Expression dest)
        {
            _source = source;
            _dest = dest;
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (lx == _source)
                return _dest;

            return base.Transform(lx);
        }

        protected override Expression Transform(ConstantExpression cx)
        {
            if (cx == _source)
                return _dest;

            return base.Transform(cx);
        }

        protected override Expression Transform(AmbientPropertyExpression px)
        {
            if (px == _source)
                return _dest;

            return base.Transform(px);
        }

        protected override Expression Transform(AccessorExpression spx)
        {
            if (spx == _source)
                return _dest;

            return base.Transform(spx);
        }

        protected override Expression Transform(LambdaExpression lmx)
        {
            if (lmx == _source)
                return _dest;

            return base.Transform(lmx);
        }

        protected override Expression Transform(ParameterExpression prx)
        {
            if (prx == _source)
                return _dest;

            return base.Transform(prx);
        }

        protected override Expression Transform(IndexerWildcardExpression wx)
        {
            if (wx == _source)
                return _dest;

            return base.Transform(wx);
        }
    }
}
