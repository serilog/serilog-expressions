using System;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Templates.Ast;
using Serilog.Templates.Compilation.UnreferencedProperties;
using Serilog.Templates.Parsing;
using Xunit;

namespace Serilog.Expressions.Tests.Templates
{
    public class UnreferencedPropertiesFunctionTests
    {
        [Fact]
        public void UnreferencedPropertiesFunctionIsNamedRest()
        {
            var function = new UnreferencedPropertiesFunction(new LiteralText("test"));
            Assert.True(function.TryResolveFunctionName("Rest", out _));
        }

        [Fact]
        public void UnreferencedPropertiesExcludeThoseInMessageAndTemplate()
        {
            Assert.True(new TemplateParser().TryParse("{A + 1}{#if true}{B}{#end}", out var template, out _));
            
            var function = new UnreferencedPropertiesFunction(template!);
            
            var evt = new LogEvent(
                DateTimeOffset.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(new[] {new PropertyToken("C", "{C}")}),
                new[]
                {
                    new LogEventProperty("A", new ScalarValue(null)),
                    new LogEventProperty("B", new ScalarValue(null)),
                    new LogEventProperty("C", new ScalarValue(null)),
                    new LogEventProperty("D", new ScalarValue(null)),
                });

            var result = UnreferencedPropertiesFunction.Implementation(function, evt);
            
            var sv = Assert.IsType<StructureValue>(result);
            var included = Assert.Single(sv.Properties);
            Assert.Equal("D", included!.Name);
        }
    }
}