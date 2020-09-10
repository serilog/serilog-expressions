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

        protected override Expression Transform(Expression x)
        {
            if (x == _source)
                return _dest;

            return base.Transform(x);
        }
    }
}
