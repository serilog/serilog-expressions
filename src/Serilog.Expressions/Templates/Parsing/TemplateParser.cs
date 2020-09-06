using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog.Expressions.Parsing;
using Serilog.Templates.Ast;
using Superpower.Model;

namespace Serilog.Templates.Parsing
{
    static class TemplateParser
    {
        static ExpressionTokenizer Tokenizer { get; } = new ExpressionTokenizer();
        
        public static Template Parse(string template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            
            var elements = new List<Template>();

            var i = 0;
            while (i < template.Length)
            {
                var ch = template[i];

                if (ch == '{')
                {
                    i++;
                    if (i == template.Length)
                        throw new ArgumentException("Character `{` must be escaped by doubling in literal text.");

                    if (template[i] == '{')
                    {
                        elements.Add(new LiteralText("}"));
                        i++;
                    }
                    else
                    {
                        // Not reporting line/column
                        var tokens = Tokenizer.GreedyTokenize(new TextSpan(template, new Position(i, 0, 0), template.Length - i));
                        // Dropping error info; may return a zero-length parse
                        var expr = ExpressionTokenParsers.TryPartialParse(tokens);
                        // Throw on error; no format parsing
                        elements.Add(new FormattedExpression(expr.Value, null));
                        if (expr.Remainder.Position == tokens.Count())
                            i = tokens.Last().Position.Absolute + tokens.Last().Span.Length;
                        else
                            i = tokens.ElementAt(i).Position.Absolute;

                        if (i == template.Length || template[i] != '}')
                            throw new ArgumentException("Un-closed hole, `}` expected.");
                        i++;
                    }
                }
                else if (ch == '}')
                {
                    i++;
                    if (i == template.Length || template[i] != '}')
                        throw new ArgumentException("Character `}` must be escaped by doubling in literal text.");
                    elements.Add(new LiteralText("}"));
                    i++;
                }
                else
                {
                    var literal = new StringBuilder();
                    do
                    {
                        literal.Append(template[i]);
                        i++;
                    } while (i < template.Length && template[i] != '{' && template[i] != '}');
                    elements.Add(new LiteralText(literal.ToString()));
                }
            }
            
            if (elements.Count == 1)
                return elements.Single();
            
            return new TemplateBlock(elements.ToArray());
        }
    }
}
