using System.Collections.Generic;
using Superpower;
using Superpower.Model;

namespace Serilog.Expressions.Parsing
{
    class ExpressionTokenizer : Tokenizer<ExpressionToken>
    {
        readonly bool _templateSyntax;
        
        static readonly ExpressionToken[] SimpleOps = new ExpressionToken[128];

        static readonly ExpressionKeyword[] Keywords =
        {
            new ExpressionKeyword("and", ExpressionToken.And),
            new ExpressionKeyword("in", ExpressionToken.In),
            new ExpressionKeyword("is", ExpressionToken.Is),
            new ExpressionKeyword("like", ExpressionToken.Like),
            new ExpressionKeyword("not", ExpressionToken.Not),
            new ExpressionKeyword("or", ExpressionToken.Or),
            new ExpressionKeyword("true", ExpressionToken.True),
            new ExpressionKeyword("false", ExpressionToken.False),
            new ExpressionKeyword("null", ExpressionToken.Null)
        };

        static ExpressionTokenizer()
        {
            SimpleOps['+'] = ExpressionToken.Plus;
            SimpleOps['-'] = ExpressionToken.Minus;
            SimpleOps['*'] = ExpressionToken.Asterisk;
            SimpleOps['/'] = ExpressionToken.ForwardSlash;
            SimpleOps['%'] = ExpressionToken.Percent;
            SimpleOps['^'] = ExpressionToken.Caret;
            SimpleOps['<'] = ExpressionToken.LessThan;
            SimpleOps['>'] = ExpressionToken.GreaterThan;
            SimpleOps['='] = ExpressionToken.Equal;
            SimpleOps[','] = ExpressionToken.Comma;
            SimpleOps['.'] = ExpressionToken.Period;
            SimpleOps['('] = ExpressionToken.LParen;
            SimpleOps[')'] = ExpressionToken.RParen;
            SimpleOps['['] = ExpressionToken.LBracket;
            SimpleOps[']'] = ExpressionToken.RBracket;
            SimpleOps['*'] = ExpressionToken.Asterisk;
            SimpleOps['?'] = ExpressionToken.QuestionMark;
        }

        public ExpressionTokenizer(bool templateSyntax)
        {
            _templateSyntax = templateSyntax;
        }

        protected override IEnumerable<Result<ExpressionToken>> Tokenize(TextSpan stringSpan)
        {
            return _templateSyntax ? TokenizeTemplate(stringSpan) : TokenizeExpression(stringSpan);
        }

        IEnumerable<Result<ExpressionToken>> TokenizeTemplate(TextSpan stringSpan)
        {
            var last = stringSpan;
            var next = SkipLiteral(stringSpan);
            if (!next.HasValue)
                yield break;

            do
            {
                yield return Result.Value(ExpressionToken.TemplateLiteral, last, next.Location);

                if (next.Value == '{')
                {
                    next = next.Remainder.ConsumeChar();
                    if (!next.HasValue)
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location, "Unexpected end-of-input.");
                        yield break;
                    }

                    if (next.Value == '{')
                    {
                        yield return Result.Value(ExpressionToken.TemplateLiteral, next.Location, next.Remainder);
                    }
                    else
                    {
                        var until = next.Location;
                        
                        // Here we'll need to contextually apply the parser...
                        foreach (var tok in TokenizeExpression(next.Location))
                        {
                            yield return tok;
                            until = tok.Remainder;
                        }
                        
                        next = until.ConsumeChar();
                        if (!next.HasValue)
                        {
                            yield return Result.Empty<ExpressionToken>(next.Location, "Unexpected end-of-input.");
                            yield break;
                        }
                    }
                }
                else if (next.Value == '}')
                {
                    
                }

                last = next.Remainder;
                next = SkipLiteral(next.Remainder);
            } while (next.HasValue);
        }

        static Result<char> SkipLiteral(TextSpan stringSpan)
        {
            Result<char> result = stringSpan.ConsumeChar();
            while (result.HasValue && result.Value != '{' && result.Value != '}')
                result = result.Remainder.ConsumeChar();
            return result;
        }
        
        IEnumerable<Result<ExpressionToken>> TokenizeExpression(TextSpan stringSpan)
        {
            var next = SkipWhiteSpace(stringSpan);
            if (!next.HasValue)
                yield break;

            do
            {
                if (char.IsDigit(next.Value))
                {
                    var hex = ExpressionTextParsers.HexInteger(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(ExpressionToken.HexNumber, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = ExpressionTextParsers.Real(next.Location);
                        if (!real.HasValue)
                            yield return Result.CastEmpty<TextSpan, ExpressionToken>(real);
                        else
                            yield return Result.Value(ExpressionToken.Number, real.Location, real.Remainder);

                        next = real.Remainder.ConsumeChar();
                    }

                    if (!IsDelimiter(next))
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '\'')
                {
                    var str = ExpressionTextParsers.String(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, ExpressionToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(ExpressionToken.String, str.Location, str.Remainder);
                }
                else if (next.Value == '@')
                {
                    var beginIdentifier = next.Location;
                    var startOfName = next.Remainder;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && char.IsLetterOrDigit(next.Value));

                    if (next.Remainder == startOfName)
                    {
                        yield return Result.Empty<ExpressionToken>(startOfName, new[] { "built-in identifier name" });
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.BuiltInIdentifier, beginIdentifier, next.Location);
                    }
                }
                else if (char.IsLetter(next.Value) || next.Value == '_')
                {
                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (char.IsLetterOrDigit(next.Value) || next.Value == '_'));

                    if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                    {
                        yield return Result.Value(keyword, beginIdentifier, next.Location);
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else
                {
                    var compoundOp = ExpressionTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != ExpressionToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        static bool IsDelimiter(Result<char> next)
        {
            return !next.HasValue ||
                   char.IsWhiteSpace(next.Value) ||
                   next.Value < SimpleOps.Length && SimpleOps[next.Value] != ExpressionToken.None;
        }

        static bool TryGetKeyword(TextSpan span, out ExpressionToken keyword)
        {
            foreach (var kw in Keywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = ExpressionToken.None;
            return false;
        }
    }
}
