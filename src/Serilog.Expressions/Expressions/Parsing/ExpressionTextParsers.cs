using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Serilog.Expressions.Parsing
{
    static class ExpressionTextParsers
    {
        static readonly TextParser<ExpressionToken> LessOrEqual = Span.EqualTo("<=").Value(ExpressionToken.LessThanOrEqual);
        static readonly TextParser<ExpressionToken> GreaterOrEqual = Span.EqualTo(">=").Value(ExpressionToken.GreaterThanOrEqual);
        static readonly TextParser<ExpressionToken> NotEqual = Span.EqualTo("<>").Value(ExpressionToken.NotEqual);

        public static TextParser<ExpressionToken> CompoundOperator = GreaterOrEqual.Or(LessOrEqual.Try().Or(NotEqual));

        public static TextParser<string> HexInteger =
            Span.EqualTo("0x")
                .IgnoreThen(Character.Digit.Or(Character.Matching(ch => ch >= 'a' && ch <= 'f' || ch >= 'A' && ch <= 'F', "a-f"))
                    .Named("hex digit")
                    .AtLeastOnce())
                .Select(chars => new string(chars));

        public static TextParser<char> StringContentChar =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\'', '\r', '\n'));

        public static TextParser<string> String =
            Character.EqualTo('\'')
                .IgnoreThen(StringContentChar.Many())
                .Then(s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<TextSpan> Real =
            Numerics.Integer
                .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));
    }
}
