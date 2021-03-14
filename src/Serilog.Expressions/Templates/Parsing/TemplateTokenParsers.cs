using Serilog.Expressions.Ast;
using Serilog.Expressions.Parsing;
using Serilog.Parsing;
using Serilog.Templates.Ast;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using static Serilog.Expressions.Parsing.ExpressionToken;

// ReSharper disable SuggestBaseTypeForParameter, ConvertIfStatementToSwitchStatement, AccessToModifiedClosure

namespace Serilog.Templates.Parsing
{
    class TemplateTokenParsers
    {
        readonly TokenListParser<ExpressionToken, Template> _template;

        public TemplateTokenParsers()
        {
            TokenListParser<ExpressionToken, Template>? block = null;

            var alignment =
                Token.EqualTo(Comma).IgnoreThen(
                    (from direction in Token.EqualTo(Minus).Value(AlignmentDirection.Left)
                            .OptionalOrDefault(AlignmentDirection.Right)
                        from width in Token.EqualTo(Number).Apply(Numerics.NaturalUInt32)
                        select new Alignment(direction, (int) width)).Named("alignment and width"));

            var format = Token.EqualTo(Colon)
                .IgnoreThen(Token.EqualTo(Format))
                .Select(fmt => (string?) fmt.ToStringValue());

            var hole =
                from _ in Token.EqualTo(LBrace)
                from expr in ExpressionTokenParsers.Expr
                from align in alignment.OptionalOrDefault()
                from fmt in format.OptionalOrDefault()
                from __ in Token.EqualTo(RBrace)
                select (Template) new FormattedExpression(expr, fmt, align);

            static TokenListParser<ExpressionToken, Expression?> Directive(
                bool hasArgument,
                params ExpressionToken[] signifiers)
            {
                var open = Token.EqualTo(LBraceHash)
                    .IgnoreThen(Token.Sequence(signifiers)).Try();

                if (hasArgument)
                    return open
                        .IgnoreThen(ExpressionTokenParsers.Expr.Cast<ExpressionToken, Expression, Expression?>())
                        .Then(v => Token.EqualTo(RBrace).Value(v));
                
                return open.IgnoreThen(Token.EqualTo(RBrace)).Value((Expression?)null);
            }

            static Template? LeftReduceConditional((Expression?, Template)[] first, Template? last)
            {
                for (var i = first.Length -1; i >= 0; i--)
                {
                    last = new Conditional(first[i].Item1!, first[i].Item2, last);
                }

                return last;
            }

            var conditional =
                from iff in Directive(true, If)
                from consequent in Parse.Ref(() => block)
                from alternatives in Directive(true, Else, If)
                    .Then(elsif => Parse.Ref(() => block).Select(b => (elsif, b)))
                    .Many()
                from final in Directive(false, Else)
                    .IgnoreThen(Parse.Ref(() => block).Select(b => ((Expression?) null, b)))
                    .OptionalOrDefault()
                from _ in Directive(false, End)
                let firstAlt = LeftReduceConditional(alternatives, final.b)
                select (Template)new Conditional(iff!, consequent, firstAlt);
            
            var element = Token.EqualTo(Text).Select(t => (Template)new LiteralText(t.ToStringValue()))
                .Or(Token.EqualTo(DoubleLBrace)
                    .Value((Template) new LiteralText("{")))
                .Or(Token.EqualTo(DoubleRBrace)
                    .Value((Template) new LiteralText("}")))
                .Or(conditional)
                .Or(hole);

            block = element.Many().Select(elements => elements.Length == 1 ?
                elements[0] :
                new TemplateBlock(elements));
            
            _template = block.AtEnd();
        }
        
        public TokenListParserResult<ExpressionToken, Template> TryParse(
            TokenList<ExpressionToken> input)
        {
            return _template.TryParse(input);
        }
    }
}
