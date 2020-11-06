using System.Collections.Generic;
using System.Linq.Expressions;
using Serilog.Events;

namespace Serilog.Expressions.Compilation.Linq
{
    class ExpressionConstantMapper : ExpressionVisitor
    {
        readonly IDictionary<object, Expression> _mapping;

        public ExpressionConstantMapper(IDictionary<object, Expression> mapping)
        {
            _mapping = mapping;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value != null && 
                node.Value is ScalarValue sv &&
                _mapping.TryGetValue(sv.Value, out var substitute))
                return substitute;

            return base.VisitConstant(node);
        }
    }
}
