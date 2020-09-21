using System;
using System.Text.RegularExpressions;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Text
{
    class TextMatchingTransformer: IdentityTransformer
    {
        static readonly TextMatchingTransformer Instance = new TextMatchingTransformer();

        public static Expression Rewrite(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (lx.Operands.Length != 2)
                return base.Transform(lx);

            if (Operators.SameOperator(lx.OperatorName, Operators.OpIndexOfMatch))
                return TryCompileIndexOfMatch(lx.IgnoreCase, lx.Operands[0], lx.Operands[1]);
            
            if (Operators.SameOperator(lx.OperatorName, Operators.OpIsMatch))
                return new CallExpression(
                    false,
                    Operators.RuntimeOpNotEqual,
                    TryCompileIndexOfMatch(lx.IgnoreCase, lx.Operands[0], lx.Operands[1]),
                    new ConstantExpression(new ScalarValue(-1)));

            return base.Transform(lx);
        }

        Expression TryCompileIndexOfMatch(bool ignoreCase, Expression corpus, Expression regex)
        {
            if (regex is ConstantExpression cx &&
                cx.Constant is ScalarValue scalar &&
                scalar.Value is string s)
            {
                var opts = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
                if (ignoreCase)
                    opts |= RegexOptions.IgnoreCase;
                var compiled = new Regex(s, opts, TimeSpan.FromMilliseconds(100));
                return new IndexOfMatchExpression(Transform(corpus), compiled);
            }
            
            SelfLog.WriteLine($"Serilog.Expressions: `IndexOfMatch()` requires a constant string regular expression argument; found ${regex}.");
            return new CallExpression(false, Operators.OpUndefined);
        }
    }
}
