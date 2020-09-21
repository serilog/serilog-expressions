using Serilog.Events;
using Serilog.Expressions.Runtime;
using Serilog.Expressions.Tests.Support;
using Xunit;

namespace Serilog.Expressions.Tests
{
    public class NameResolverTests
    {
        public static LogEventPropertyValue? Magic(LogEventPropertyValue? number)
        {
            if (!Coerce.Numeric(number, out var num))
                return null;
            
            return new ScalarValue(num + 42);
        }

        [Fact]
        public void UserDefinedFunctionsAreCallableInExpressions()
        {
            var expr = SerilogExpression.Compile(
                "magic(10) + 3 = 55",
                new[] {new StaticMemberNameResolver(typeof(NameResolverTests))});
            Assert.True(Coerce.IsTrue(expr(Some.InformationEvent())));
        }
    }
}