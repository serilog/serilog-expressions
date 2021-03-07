using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Compilation;
using Serilog.Expressions.Parsing;
using Serilog.Expressions.Runtime;
using Serilog.Expressions.Tests.Support;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class ExpressionTranslationTests
    {
        public static IEnumerable<object[]> ExpressionEvaluationCases =>
            AsvCases.ReadCases("translation-cases.asv");

        [Theory]
        [MemberData(nameof(ExpressionEvaluationCases))]
        public void ExpressionsAreCorrectlyTranslated(string expr, string expected)
        {
            var parsed = ExpressionParser.Parse(expr);
            var translated = ExpressionCompiler.Translate(parsed);
            var actual = translated.ToString();
            Assert.Equal(expected, actual);
        }
    }
}
