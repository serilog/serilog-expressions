using System;
using Superpower;
using Superpower.Model;

namespace Serilog.Expressions.Parsing
{
    static class ParserExtensions
    {
        public static TokenListParser<TTokenKind, TResult> SelectCatch<TTokenKind, TArg, TResult>(this TokenListParser<TTokenKind, TArg> parser, Func<TArg, TResult> trySelector, string errorMessage)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (trySelector == null) throw new ArgumentNullException(nameof(trySelector));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

            return input =>
            {
                var t = parser(input);
                if (!t.HasValue)
                    return TokenListParserResult.CastEmpty<TTokenKind, TArg, TResult>(t);

                try
                {
                    var u = trySelector(t.Value);
                    return TokenListParserResult.Value(u, input, t.Remainder);
                }
                catch
                {
                    return TokenListParserResult.Empty<TTokenKind, TResult>(input, errorMessage);
                }
            };
        }
    }
}