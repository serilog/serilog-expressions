using System;
using System.Linq.Expressions;

namespace Serilog.Expressions.Compilation.Linq
{
    class ParameterReplacementVisitor : ExpressionVisitor
    {
        readonly ParameterExpression[] _from, _to;

        public ParameterReplacementVisitor(ParameterExpression[] from, ParameterExpression[] to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from.Length != to.Length) throw new InvalidOperationException("Mismatched parameter lists");
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            for (var i = 0; i < _from.Length; i++)
            {
                if (node == _from[i]) return _to[i];
            }
            return node;
        }
    }
}
