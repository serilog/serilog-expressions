using System;
using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Compilation.Transformations
{
    abstract class SerilogExpressionTransformer<TResult>
    {
        protected TResult Transform(Expression expression)
        {
            return expression switch
            {
                CallExpression call => Transform(call),
                ConstantExpression constant => Transform(constant),
                AccessorExpression accessor => Transform(accessor),
                AmbientPropertyExpression property => Transform(property),
                LambdaExpression lambda => Transform(lambda),
                ParameterExpression parameter => Transform(parameter),
                IndexerWildcardExpression wildcard => Transform(wildcard),
                ArrayExpression array => Transform(array),
                IndexerExpression indexer => Transform(indexer),
                IndexOfMatchExpression match => Transform(match),
                null => throw new ArgumentNullException(nameof(expression)),
                // Non-exhaustive because `InternalsVisibleTo` is applied to the assembly.
                _ => throw new NotSupportedException($"{expression} is not supported.")
            };
        }

        protected abstract TResult Transform(CallExpression lx);
        protected abstract TResult Transform(ConstantExpression cx);
        protected abstract TResult Transform(AmbientPropertyExpression px);
        protected abstract TResult Transform(AccessorExpression spx);
        protected abstract TResult Transform(LambdaExpression lmx);
        protected abstract TResult Transform(ParameterExpression prx);
        protected abstract TResult Transform(IndexerWildcardExpression wx);
        protected abstract TResult Transform(ArrayExpression ax);
        protected abstract TResult Transform(IndexerExpression ix);
        protected abstract TResult Transform(IndexOfMatchExpression mx);
    }
}
