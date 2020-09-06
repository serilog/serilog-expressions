using System;
using System.Text.RegularExpressions;

namespace Serilog.Expressions.Ast
{
    class IndexOfMatchExpression : Expression
    {
        public Expression Corpus { get; }
        public Regex Regex { get; }

        public IndexOfMatchExpression(Expression corpus, Regex regex)
        {
            Corpus = corpus ?? throw new ArgumentNullException(nameof(corpus));
            Regex = regex ?? throw new ArgumentNullException(nameof(regex));
        }
    }
}
