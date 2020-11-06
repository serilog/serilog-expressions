using System;

namespace Serilog.Templates.Ast
{
    class LiteralText : Template
    {
        public string Text { get; }

        public LiteralText(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}