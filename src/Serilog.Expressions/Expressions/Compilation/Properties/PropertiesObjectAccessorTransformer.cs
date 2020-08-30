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

        protected override Expression Transform(CallExpression lx)
        {
            if (!Operators.SameOperator(lx.OperatorName, Operators.OpElementAt) || lx.Operands.Length != 2)
                return base.Transform(lx);

            if (!(lx.Operands[0] is AmbientPropertyExpression p)
                    || !(lx.Operands[1] is ConstantExpression n)
                    || !p.IsBuiltIn 
                    || p.PropertyName != "Properties"
                    || !(n.ConstantValue is string name))
                return base.Transform(lx);

            return new AmbientPropertyExpression(name, false);
        }
    }
}
