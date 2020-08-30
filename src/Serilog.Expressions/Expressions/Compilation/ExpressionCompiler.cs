using System;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Arrays;
using Serilog.Expressions.Compilation.Costing;
using Serilog.Expressions.Compilation.In;
using Serilog.Expressions.Compilation.Is;
using Serilog.Expressions.Compilation.Linq;
using Serilog.Expressions.Compilation.Properties;
using Serilog.Expressions.Compilation.Wildcards;
using Serilog.Expressions.Runtime;

namespace Serilog.Expressions.Compilation
{
    static class ExpressionCompiler
    {
        public static Func<LogEvent, object> CompileAndExpose(Expression expression)
        {
            var actual = expression;
            actual = PropertiesObjectAccessorTransformer.Rewrite(actual);
            actual = ConstantArrayEvaluator.Evaluate(actual);
            actual = NotInRewriter.Rewrite(actual);
            actual = WildcardComprehensionTransformer.Expand(actual);
            actual = IsOperatorTransformer.Rewrite(actual);
            actual = EvaluationCostReordering.Reorder(actual);

            var compiled = LinqExpressionCompiler.Compile(actual);
            return ctx =>
            {
                var result = compiled(ctx);
                return Representation.Expose(result);
            };
        }
    }
}
