using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Expressions.Runtime;
using Serilog.Expressions.Tests.Support;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class ExpressionEvaluationTests
    {
        static readonly string CasesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Cases");

        static IEnumerable<object[]> ReadCases(string filename)
        {
            foreach (var line in File.ReadLines(Path.Combine(CasesPath, filename)))
            {
                var cols = line.Split("⇶", StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length == 2)
                    yield return cols.Select(c => c.Trim()).ToArray<object>();
            }
        }

        public static IEnumerable<object[]> EphemeralExpressionEvaluationCases =>
            ReadCases("ephemeral-expression-evaluation-cases.asv");

        [Theory]
        [MemberData(nameof(EphemeralExpressionEvaluationCases))]
        public void EphemeralExpressionsAreCorrectlyEvaluated(string expr, string result)
        {
            var actual = SerilogExpression.Compile(expr)(Some.InformationEvent());
            var expected = SerilogExpression.Compile(result)(Some.InformationEvent());

            if (expected is null)
            {
                Assert.True(actual is null, $"Expected: undefined{Environment.NewLine}Actual: {actual}");
            }
            else
            {
                Assert.True(RuntimeOperators.Equal(actual, expected)?.Equals(true), $"Expected: {expected}{Environment.NewLine}Actual: {actual}");
            }
        }
    }
}