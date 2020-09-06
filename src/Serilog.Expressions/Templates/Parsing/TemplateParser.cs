using System.Collections.Generic;
using System.Linq;
using Serilog.Expressions.Parsing;
using Serilog.Templates.Ast;

namespace Serilog.Templates.Parsing
{
    static class TemplateParser
    {
        static ExpressionTokenizer Tokenizer { get; } = new ExpressionTokenizer(true);
        
        public static Template Parse(string template)
        {
            var tokens = Tokenizer.Tokenize(template);

            var elements = new List<Template>();

            var rest = tokens;
            while (!rest.IsAtEnd)
            {
                var peek = rest.ConsumeToken();
                if (peek.Value.Kind == ExpressionToken.TemplateLiteral)
                {
                    elements.Add(new LiteralText(peek.Value.ToStringValue()));
                    rest = peek.Remainder;
                }
                else
                {
                    var partial = ExpressionTokenParsers.TryPartialParse(rest);
                    elements.Add(new FormattedExpression(partial.Value, null));
                    rest = partial.Remainder;
                }
            }

            if (elements.Count == 1)
                return elements.Single();
            
            return new TemplateBlock(elements.ToArray());
        }
    }
}
