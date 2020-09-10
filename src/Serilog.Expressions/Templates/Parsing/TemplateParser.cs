using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        
        public static bool TryParse(
            string template,
            [MaybeNullWhen(false)] out Template parsed, 
            [MaybeNullWhen(true)] out string error)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            parsed = null;
            var elements = new List<Template>();

            var i = 0;
            while (i < template.Length)
            {
                var ch = template[i];

                if (ch == '{')
                {
                    i++;
                    if (i == template.Length)
                    {
                        error = "Character `{` must be escaped by doubling in literal text.";
                        return false;
                    }

                    if (template[i] == '{')
                    {
                        elements.Add(new LiteralText("{"));
                        i++;
                    }
                    else
                    {
                        // No line/column tracking
                        var tokens = Tokenizer.GreedyTokenize(new TextSpan(template, new Position(i, 0, 0), template.Length - i));
                        var expr = ExpressionTokenParsers.TryPartialParse(tokens);
                        if (!expr.HasValue)
                        {
                            // Error message accuracy is not great here
                            error = $"Invalid expression, {expr.FormatErrorMessageFragment()}.";
                            return false;
                        }

                        if (expr.Remainder.Position == tokens.Count())
                            i = tokens.Last().Position.Absolute + tokens.Last().Span.Length;
                        else
                            i = tokens.ElementAt(expr.Remainder.Position).Position.Absolute;

                        if (i == template.Length)
                        {
                            error = "Un-closed hole, `}` expected.";
                            return false;
                        }

                        string? format = null;
                        if (template[i] == ':')
                        {
                            i++;
                            
                            var formatBuilder = new StringBuilder();
                            while (i < template.Length && template[i] != '}')
                            {
                                formatBuilder.Append(template[i]);
                                i++;
                            }

                            format = formatBuilder.ToString();
                        }

                        if (i == template.Length)
                        {
                            error = "Un-closed hole, `}` expected.";
                            return false;
                        }

                        if (template[i] != '}')
                        {
                            error = $"Invalid expression, unexpected `{template[i]}`.";
                            return false;
                        }

                        i++;
                        
                        elements.Add(new FormattedExpression(expr.Value, format));
                    }
                }
                else if (ch == '}')
                {
                    i++;
                    if (i == template.Length || template[i] != '}')
                    {
                        error = "Character `}` must be escaped by doubling in literal text.";
                        return false;
                    }

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
                parsed = elements.Single();
            else
                parsed = new TemplateBlock(elements.ToArray());

            error = null;
            return true;
        }
    }
}
