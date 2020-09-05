using System.Collections.Generic;
using Serilog.Events;
using System.Linq;
using Serilog.Expressions.Tests.Support;
using Xunit;

// ReSharper disable CoVariantArrayConversion

namespace Serilog.Expressions.Tests
{
    public class ExpressionCompilerTests
    {
        [Fact]
        public void ExpressionsEvaluateStringEquality()
        {
            AssertEvaluation("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent(),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ComparisonsAreCaseSensitive()
        {
            AssertEvaluation("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "APPLE"));
        }

        [Fact]
        public void ExpressionsEvaluateStringContent()
        {
            AssertEvaluation("Fruit like '%pp%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ExpressionsEvaluateStringPrefix()
        {
            AssertEvaluation("Fruit like 'Ap%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ExpressionsEvaluateStringSuffix()
        {
            AssertEvaluation("Fruit like '%le'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void LikeIsCaseInsensitive()
        {
            AssertEvaluation("Fruit like 'apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"));
        }

        [Fact]
        public void ExpressionsEvaluateNumericComparisons()
        {
            AssertEvaluation("Volume > 11",
                Some.InformationEvent("Adding {Volume} L", 11.5),
                Some.InformationEvent("Adding {Volume} L", 11));
        }

        [Fact]
        public void ExpressionsEvaluateWildcardsOnCollectionItems()
        {
            AssertEvaluation("Items[?] like 'C%'",
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Tea", "Coffee" } }), // Test helper doesn't correct this case
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Apricots" } }));
        }

        [Fact]
        public void ExpressionsEvaluateBuiltInProperties()
        {
            AssertEvaluation("@Level = 'Information'",
                Some.InformationEvent(),
                Some.WarningEvent());
        }

        [Fact]
        public void ExpressionsEvaluateExistentials()
        {
            AssertEvaluation("AppId is not null",
                Some.InformationEvent("{AppId}", 10),
                Some.InformationEvent("{AppId}", null),
                Some.InformationEvent());
        }

        [Fact]
        public void ExpressionsLogicalOperations()
        {
            AssertEvaluation("A and B",
                Some.InformationEvent("{A} {B}", true, true),
                Some.InformationEvent("{A} {B}", true, false),
                Some.InformationEvent());
        }

        [Fact]
        public void ExpressionsEvaluateSubproperties()
        {
            AssertEvaluation("Cart.Total > 10",
                Some.InformationEvent("Checking out {@Cart}", new { Total = 20 }),
                Some.InformationEvent("Checking out {@Cart}", new { Total = 5 }));
        }


        [Fact]
        public void SequenceLengthCanBeDetermined()
        {
            AssertEvaluation("length(Items) > 1",
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears", "apples" }}),
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears" }}));
        }

        [Fact]
        public void InMatchesLiterals()
        {
            AssertEvaluation("@Level in ['Warning', 'Error']",
                Some.LogEvent(LogEventLevel.Error, "Hello"),
                Some.InformationEvent("Hello"));
        }

        [Fact]
        public void InExaminesSequenceValues()
        {
            AssertEvaluation("5 not in Numbers",
                Some.InformationEvent("{Numbers}", new []{1, 2, 3}),
                Some.InformationEvent("{Numbers}", new [] { 1, 5, 3 }),
                Some.InformationEvent());
        }

        static void AssertEvaluation(string expression, LogEvent match, params LogEvent[] noMatches)
        {
            var sink = new CollectingSink();

            var log = new LoggerConfiguration()
                .Filter.ByIncludingOnly(expression)
                .WriteTo.Sink(sink)
                .CreateLogger();

            foreach (var noMatch in noMatches)
                log.Write(noMatch);

            log.Write(match);

            Assert.Single(sink.Events);
            Assert.Same(match, sink.Events.Single());
        }

        [Fact]
        public void StructuresAreExposedAsDictionaries()
        {
            var evt = Some.InformationEvent("{@Person}", new { Name = "nblumhardt" });
            var expr = SerilogExpression.Compile("Person");
            var val = expr(evt);
            var dict = Assert.IsType<Dictionary<string, object>>(val);
            Assert.Equal("nblumhardt", dict["Name"]);
        }
    }
}
