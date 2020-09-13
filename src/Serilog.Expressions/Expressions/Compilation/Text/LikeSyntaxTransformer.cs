using System;
using System.Text.RegularExpressions;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Text
{
    class LikeSyntaxTransformer: IdentityTransformer
    {
        static readonly LikeSyntaxTransformer Instance = new LikeSyntaxTransformer();

        public static Expression Rewrite(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(CallExpression lx)
        {
            if (lx.Operands.Length != 2)
                return base.Transform(lx);

            if (Operators.SameOperator(lx.OperatorName, Operators.IntermediateOpLike))
                return TryCompileLikeExpression(lx.Operands[0], lx.Operands[1]);
            
            if (Operators.SameOperator(lx.OperatorName, Operators.IntermediateOpNotLike))
                return new CallExpression(
                    Operators.RuntimeOpStrictNot,
                    TryCompileLikeExpression(lx.Operands[0], lx.Operands[1]));

            return base.Transform(lx);
        }

        Expression TryCompileLikeExpression(Expression corpus, Expression like)
        {
            if (like is ConstantExpression cx &&
                cx.Constant is ScalarValue scalar &&
                scalar.Value is string s)
            {
                var regex = LikeToRegex(s);
                var compiled = new Regex(regex, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(100));
                var indexof = new IndexOfMatchExpression(Transform(corpus), compiled);
                return new CallExpression(Operators.OpNotEqual, indexof, new ConstantExpression(new ScalarValue(-1)));
            }
            
            SelfLog.WriteLine($"Serilog.Expressions: `like` requires a constant string argument; found ${like}.");
            return new AmbientPropertyExpression(BuiltInProperty.Undefined, true);
        }
        
        static string LikeToRegex(string like)
        {
            var regex = "";
            for (var i = 0; i < like.Length; ++i)
            {
                var ch = like[i];
                char? following = i == like.Length - 1 ? (char?)null : like[i + 1];
                if (ch == '%')
                {
                    if (following == '%')
                    {
                        regex += '%';
                        ++i;
                    }
                    else
                    {
                        regex += "(?:.|\\r|\\n)*"; // ~= RegexOptions.Singleline
                    }
                }
                else if (ch == '_')
                {
                    if (following == '_')
                    {
                        regex += '_';
                        ++i;
                    }
                    else
                    {
                        regex += '.'; // Newlines aren't considered matches for _
                    }
                }
                else
                    regex += Regex.Escape(ch.ToString());
            }

            return regex;
        }
    }
}
