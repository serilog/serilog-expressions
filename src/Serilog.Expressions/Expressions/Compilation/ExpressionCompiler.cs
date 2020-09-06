using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Arrays;
using Serilog.Expressions.Compilation.In;
using Serilog.Expressions.Compilation.Is;
using Serilog.Expressions.Compilation.Linq;
using Serilog.Expressions.Compilation.Properties;
using Serilog.Expressions.Compilation.Text;
using Serilog.Expressions.Compilation.Wildcards;

namespace Serilog.Expressions.Compilation
{
    static class ExpressionCompiler
    {
        public static CompiledExpression Compile(Expression expression)
        {
            var actual = expression;
            actual = TextMatchingTransformer.Rewrite(actual);
            actual = LikeSyntaxTransformer.Rewrite(actual);
            actual = PropertiesObjectAccessorTransformer.Rewrite(actual);
            actual = ConstantArrayEvaluator.Evaluate(actual);
            actual = NotInRewriter.Rewrite(actual); 
            actual = WildcardComprehensionTransformer.Expand(actual);
            actual = IsOperatorTransformer.Rewrite(actual);

            return LinqExpressionCompiler.Compile(actual);
        }
    }
}
