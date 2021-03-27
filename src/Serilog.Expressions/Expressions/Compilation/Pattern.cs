using System.Diagnostics.CodeAnalysis;
using Serilog.Events;
using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Compilation
{
    static class Pattern
    {
        public static bool IsAmbientProperty(Expression expression, string name, bool isBuiltIn)
        {
            return expression is AmbientNameExpression px &&
                   px.PropertyName == name &&
                   px.IsBuiltIn == isBuiltIn;
        }

        public static bool IsStringConstant(Expression expression, [MaybeNullWhen(false)] out string value)
        {
            if (expression is ConstantExpression cx &&
                cx.Constant is ScalarValue sv &&
                sv.Value is string s)
            {
                value = s;
                return true;
            }

            value = null;
            return false;
        }
    }
}
