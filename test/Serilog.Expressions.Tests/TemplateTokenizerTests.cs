using System.Collections.Generic;
using System.Linq;
using Serilog.Expressions.Parsing;
using Serilog.Templates.Parsing;
using Xunit;

using static Serilog.Expressions.Parsing.ExpressionToken;

namespace Serilog.Expressions.Tests
{
    public class TemplateTokenizerTests
    {
        public static IEnumerable<object[]> ValidCases
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "aa",
                        new[] {Text}
                    },
                    new object[]
                    {
                        "{bb}",
                        new[] {LBrace, Identifier, RBrace}
                    },
                    new object[]
                    {
                        "aa{bb}",
                        new[] {Text, LBrace, Identifier, RBrace}
                    },
                    new object[]
                    {
                        "aa{{bb}}",
                        new[] {Text, LBraceEscape, Text, RBraceEscape}
                    },
                    new object[]
                    {
                        "{ {b: b} }c",
                        new[] {LBrace, LBrace, Identifier, Colon, Identifier, RBrace, RBrace, Text}
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCases))]
        public void ValidTemplatesAreTokenized(string template, object expected)
        {
            var expectedTokens = (ExpressionToken[]) expected;
            var tokenizer = new TemplateTokenizer();
            var actual = tokenizer.Tokenize(template).Select(t => t.Kind);
            Assert.Equal(expectedTokens, actual);
        }

        [Theory]
        [InlineData("aa{{bb}", "unexpected end of input, expected escaped `}`")]
        [InlineData("aa{ {b: 'b} }", "unexpected end of input, expected `'`")]
        public void InvalidTemplatesAreReported(string template, string fragment)
        {
            var tokenizer = new TemplateTokenizer();
            var err = tokenizer.TryTokenize(template);
            Assert.False(err.HasValue);
            Assert.Equal(fragment, err.FormatErrorMessageFragment());
        }
    }
}