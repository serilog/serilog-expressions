using System.Globalization;
using Serilog.Expressions.Tests.Support;
using Serilog.Templates;
using Xunit;

namespace Serilog.Expressions.Tests;

public class TemplateEvaluationTests
{
    public static IEnumerable<object[]> TemplateEvaluationCases =>
        AsvCases.ReadCases("template-evaluation-cases.asv");

    [Theory]
    [MemberData(nameof(TemplateEvaluationCases))]
    public void TemplatesAreCorrectlyEvaluated(string template, string expected)
    {
        var evt = Some.InformationEvent("Hello, {Name}!", "nblumhardt");
        var frFr = CultureInfo.GetCultureInfoByIetfLanguageTag("fr-FR");
        var compiled = new ExpressionTemplate(template, formatProvider: frFr);
        var output = new StringWriter();
        compiled.Format(evt, output);
        var actual = output.ToString();
        Assert.Equal(expected, actual);
    }
}