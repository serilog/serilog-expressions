using Serilog.Templates;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class TemplateParserTests
    {
        [Theory]
        [InlineData("Trailing {", "Character `{` must be escaped by doubling in literal text.")]
        [InlineData("Lonely } bracket", "Character `}` must be escaped by doubling in literal text.")]
        [InlineData("Trailing }", "Character `}` must be escaped by doubling in literal text.")]
        [InlineData("Unclosed {hole", "Un-closed hole, `}` expected.")]
        [InlineData("Syntax {+Err}or", "Invalid expression, unexpected operator `+`, expected expression.")]
        [InlineData("Syntax {1 + 2 and}or", "Invalid expression, unexpected `}`, expected expression.")]
        [InlineData("Missing {Align,-} digits", "Incomplete alignment specifier, expected digits.")]
        [InlineData("Non-digit {Align,x} specifier", "Invalid alignment specifier, expected digits.")]
        [InlineData("Empty {Align,} digits", "Incomplete alignment specifier, expected width.")]
        public void ErrorsAreReported(string input, string error)
        {
            Assert.False(ExpressionTemplate.TryParse(input, null, null, out _, out var actual));
            Assert.Equal(error, actual);
        }
    }
}