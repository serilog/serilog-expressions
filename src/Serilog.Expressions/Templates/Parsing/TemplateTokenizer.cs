using System;
using System.Collections.Generic;
using Serilog.Expressions.Parsing;
using Superpower;
using Superpower.Model;

namespace Serilog.Templates.Parsing
{
    class TemplateTokenizer : Tokenizer<ExpressionToken>
    {
        readonly ExpressionTokenizer _expressionTokenizer = new ExpressionTokenizer();

        protected override IEnumerable<Result<ExpressionToken>> Tokenize(TextSpan span)
        {
            var start = span;
            var rem = start;
            do
            {
                var next = rem.ConsumeChar();
                if (!next.HasValue)
                {
                    if (rem != start)
                        yield return Result.Value(ExpressionToken.Text, start, rem);

                    yield break;
                }

                if (next.Value == '{')
                {
                    if (rem != start)
                        yield return Result.Value(ExpressionToken.Text, start, rem);
                    
                    var peek = next.Remainder.ConsumeChar();
                    if (peek.HasValue && peek.Value == '{')
                    {
                        yield return Result.Value(ExpressionToken.LBraceEscape, next.Location, peek.Remainder);
                        start = rem = peek.Remainder;
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.LBrace, next.Location, next.Remainder);
                        start = rem = next.Remainder;

                        foreach (var token in TokenizeHole(rem))
                        {
                            yield return token;
                            start = rem = token.Remainder;
                        }
                    }
                }
                else if (next.Value == '}')
                {
                    if (rem != start)
                        yield return Result.Value(ExpressionToken.Text, start, rem);

                    var peek = next.Remainder.ConsumeChar();
                    if (peek.HasValue && peek.Value == '}')
                    {
                        yield return Result.Value(ExpressionToken.RBraceEscape, next.Location, peek.Remainder);
                        start = rem = peek.Remainder;
                    }
                    else
                    {
                        yield return Result.Empty<ExpressionToken>(next.Remainder, new[] {"escaped `}`"});
                        yield break;
                    }
                }
                else
                {
                    rem = next.Remainder;
                }
            } while (true);
        }

        IEnumerable<Result<ExpressionToken>> TokenizeHole(TextSpan span)
        {
            // Stack braces, brackets, and parens.
            // If we hit , or :, the stack is empty, and everything we've seen is balanced, we switch into
            // aligmment/width tokenization.
            // If we hit } and the stack is empty, and everything we've seen is balanced, we yield the final
            // '}' and return to literal text mode.

            var toMatch = new Stack<ExpressionToken>();
            var unbalanced = false;
            
            foreach (var token in _expressionTokenizer.LazyTokenize(span))
            {
                if (unbalanced)
                    yield break;

                yield return token;

                if (!token.HasValue)
                    break;

                if (token.Value == ExpressionToken.LParen ||
                    token.Value == ExpressionToken.LBrace ||
                    token.Value == ExpressionToken.LBracket)
                {
                    toMatch.Push(token.Value);
                }
                else if (toMatch.Count > 0)
                {
                    if (token.Value == ExpressionToken.RParen)
                    {
                        if (toMatch.Peek() != ExpressionToken.LParen)
                            unbalanced = true;
                        else
                            toMatch.Pop();
                    }
                    else if (token.Value == ExpressionToken.RBrace)
                    {
                        if (toMatch.Peek() != ExpressionToken.LBrace)
                            unbalanced = true;
                        else
                            toMatch.Pop();
                    }
                    else if (token.Value == ExpressionToken.RBracket)
                    {
                        if (toMatch.Peek() != ExpressionToken.LBracket)
                            unbalanced = true;
                        else
                            toMatch.Pop();
                    }
                }
                else if (token.Value == ExpressionToken.RBrace)
                {
                    break;
                }
                else if (token.Value == ExpressionToken.Comma ||
                         token.Value == ExpressionToken.Colon)
                {
                    throw new NotImplementedException("Tokenize alignment/format.");
                }
            }
        }
    }
}