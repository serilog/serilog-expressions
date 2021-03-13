using Serilog.Expressions.Parsing;
using Serilog.Parsing;
using Serilog.Templates.Ast;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using static Serilog.Expressions.Parsing.ExpressionToken;

namespace Serilog.Templates.Parsing
{
    class TemplateTokenParsers
    {
        readonly TokenListParser<ExpressionToken, Template> _template;

        public TemplateTokenParsers()
        {
            var alignment =
                Token.EqualTo(Comma).IgnoreThen(
                    (from direction in Token.EqualTo(Minus).Value(AlignmentDirection.Left)
                            .OptionalOrDefault(AlignmentDirection.Right)
                        from width in Token.EqualTo(Number).Apply(Numerics.NaturalUInt32)
                        select new Alignment(direction, (int)width)).Named("alignment and width"));
                    
            var format = Token.EqualTo(Colon)
                .IgnoreThen(Token.EqualTo(Format))
                .Select(fmt => (string?)fmt.ToStringValue());
            
            var hole =
                from _ in Token.EqualTo(LBrace)
                from expr in ExpressionTokenParsers.Expr
                from align in alignment.OptionalOrDefault()
                from fmt in format.OptionalOrDefault()
                from __ in Token.EqualTo(RBrace)
                select (Template) new FormattedExpression(expr, fmt, align);
            
            var element = Token.EqualTo(Text).Select(t => (Template)new LiteralText(t.ToStringValue()))
                .Or(Token.EqualTo(DoubleLBrace)
                    .Value((Template) new LiteralText("{")))
                .Or(Token.EqualTo(DoubleRBrace)
                    .Value((Template) new LiteralText("}")))
                .Or(hole);

            var block = element.Many().Select(elements => (Template) new TemplateBlock(elements));
            
            _template = block.AtEnd();
        }
        
        public TokenListParserResult<ExpressionToken, Template> TryParse(
            TokenList<ExpressionToken> input)
        {
            return _template.TryParse(input);
        }
    }
}
