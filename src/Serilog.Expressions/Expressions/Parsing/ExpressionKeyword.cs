using System;

namespace Serilog.Expressions.Parsing
{
    readonly struct ExpressionKeyword
    {
        public string Text { get; }
        public ExpressionToken Token { get; }

        public ExpressionKeyword(string text, ExpressionToken token)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Token = token;
        }
    }
}
