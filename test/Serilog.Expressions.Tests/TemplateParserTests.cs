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
        [InlineData("Syntax {1 + 2 and}or", "Invalid expression, unexpected end of input, expected expression.")]
        public void ErrorsAreReported(string input, string error)
        {
            Assert.False(OutputTemplate.TryParse(input, null, out _, out var actual));
            Assert.Equal(error, actual);
        }
    }
}