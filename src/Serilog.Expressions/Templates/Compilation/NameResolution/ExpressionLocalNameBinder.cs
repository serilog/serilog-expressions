using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Templates.Compilation.NameResolution
{
    class ExpressionLocalNameBinder : IdentityTransformer
    {
        readonly IReadOnlyCollection<string> _localNames;

        public static Expression BindLocalValueNames(Expression expression, IReadOnlyCollection<string> locals)
        {
            var expressionLocalNameBinder = new ExpressionLocalNameBinder(locals);
            return expressionLocalNameBinder.Transform(expression);
        }

        ExpressionLocalNameBinder(IReadOnlyCollection<string> localNames)
        {
            _localNames = localNames ?? throw new ArgumentNullException(nameof(localNames));
        }
        
        protected override Expression Transform(AmbientPropertyExpression px)
        {
            if (!px.IsBuiltIn && _localNames.Contains(px.PropertyName))
                return new NamedLocalExpression(px.PropertyName);
            
            return base.Transform(px);
        }
    }
}