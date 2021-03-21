using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Properties
{
    class PropertiesObjectAccessorTransformer : IdentityTransformer
    {
        public static Expression Rewrite(Expression actual)
        {
            return new PropertiesObjectAccessorTransformer().Transform(actual);
        }

        protected override Expression Transform(AccessorExpression ax)
        {
            if (!Pattern.IsAmbientProperty(ax.Receiver, BuiltInProperty.Properties, true))
                return base.Transform(ax);
            
            return new AmbientNameExpression(ax.MemberName, false);
        }

        protected override Expression Transform(IndexerExpression ix)
        {
            if (!Pattern.IsAmbientProperty(ix.Receiver, BuiltInProperty.Properties, true) ||
                !Pattern.IsStringConstant(ix.Index, out var name))
                return base.Transform(ix);
            
            return new AmbientNameExpression(name, false);
        }
    }
}
