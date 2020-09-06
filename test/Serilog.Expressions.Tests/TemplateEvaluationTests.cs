using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Runtime;
using Serilog.Expressions.Tests.Support;
using Serilog.Templates;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class TemplateEvaluationTests
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

        public static IEnumerable<object[]> TemplateEvaluationCases =>
            ReadCases("template-evaluation-cases.asv");

        [Theory]
        [MemberData(nameof(TemplateEvaluationCases))]
        public void TemplatesAreCorrectlyEvaluated(string template, string expected)
        {
            var evt = Some.InformationEvent();
            var compiled = new OutputTemplate(template);
            var output = new StringWriter();
            compiled.Format(evt, output);
            var actual = output.ToString();
            Assert.Equal(expected, actual);
        }
    }
}
