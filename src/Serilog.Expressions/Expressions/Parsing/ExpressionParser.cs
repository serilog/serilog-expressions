using System;
using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Parsing
{
    static class ExpressionParser
    {
        public static Expression Parse(string filterExpression)
        {
            if (!TryParse(filterExpression, out var root, out var error))
                throw new ArgumentException(error);

            return root;
        }

        public static bool TryParse(string filterExpression, out Expression root, out string error)
        {
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            var tokenList = ExpressionTokenizer.Instance.TryTokenize(filterExpression);       
            if (!tokenList.HasValue)
            {
                error = tokenList.ToString();
                root = null;
                return false;
            }

            var result = ExpressionTokenParsers.TryParse(tokenList.Value);
            if (!result.HasValue)
            {
                error = result.ToString();
                root = null;
                return false;
            }

            root = result.Value;
            error = null;
            return true;
        }
    }
}
