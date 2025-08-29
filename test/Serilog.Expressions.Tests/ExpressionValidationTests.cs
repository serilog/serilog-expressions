using Xunit;

namespace Serilog.Expressions.Tests;

public class ExpressionValidationTests
{
    [Theory]
    [InlineData("IsMatch(Name, '[invalid')", "Invalid regular expression")]
    [InlineData("IndexOfMatch(Text, '(?<')", "Invalid regular expression")]
    [InlineData("IsMatch(Name, '(?P<name>)')", "Invalid regular expression")]
    [InlineData("IsMatch(Name, '(unclosed')", "Invalid regular expression")]
    [InlineData("IndexOfMatch(Text, '*invalid')", "Invalid regular expression")]
    public void InvalidRegularExpressionsAreReportedGracefully(string expression, string expectedErrorFragment)
    {
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        Assert.False(result);
        Assert.Contains(expectedErrorFragment, error);
        Assert.Null(compiled);
    }

    [Theory]
    [InlineData("UnknownFunction()", "The function name `UnknownFunction` was not recognized.")]
    [InlineData("foo(1, 2, 3)", "The function name `foo` was not recognized.")]
    [InlineData("MyCustomFunc(Name)", "The function name `MyCustomFunc` was not recognized.")]
    [InlineData("notAFunction()", "The function name `notAFunction` was not recognized.")]
    public void UnknownFunctionNamesAreReportedGracefully(string expression, string expectedError)
    {
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        Assert.False(result);
        Assert.Equal(expectedError, error);
        Assert.Null(compiled);
    }

    [Theory]
    [InlineData("Length(Name) ci", "The function `Length` does not support case-insensitive operation.")]
    [InlineData("Round(Value, 2) ci", "The function `Round` does not support case-insensitive operation.")]
    [InlineData("Now() ci", "The function `Now` does not support case-insensitive operation.")]
    [InlineData("TypeOf(Value) ci", "The function `TypeOf` does not support case-insensitive operation.")]
    [InlineData("IsDefined(Prop) ci", "The function `IsDefined` does not support case-insensitive operation.")]
    public void InvalidCiModifierUsageIsReported(string expression, string expectedError)
    {
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        Assert.False(result);
        Assert.Equal(expectedError, error);
        Assert.Null(compiled);
    }

    [Theory]
    [InlineData("Contains(Name, 'test') ci")]
    [InlineData("StartsWith(Path, '/api') ci")]
    [InlineData("EndsWith(File, '.txt') ci")]
    [InlineData("IsMatch(Email, '@example') ci")]
    [InlineData("IndexOfMatch(Text, 'pattern') ci")]
    [InlineData("IndexOf(Name, 'x') ci")]
    [InlineData("Name = 'test' ci")]
    [InlineData("Name <> 'test' ci")]
    [InlineData("Name like '%test%' ci")]
    public void ValidCiModifierUsageCompiles(string expression)
    {
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        Assert.True(result, $"Failed to compile: {error}");
        Assert.NotNull(compiled);
        Assert.Null(error);
    }

    [Fact]
    public void MultipleErrorsAreCollectedAndReported()
    {
        var expression = "UnknownFunc() and IsMatch(Name, '[invalid') and Length(Value) ci";
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        
        Assert.False(result);
        Assert.Null(compiled);
        
        // Should report all three errors
        Assert.Contains("UnknownFunc", error);
        Assert.Contains("Invalid regular expression", error);
        Assert.Contains("does not support case-insensitive", error);
        Assert.Contains("Multiple errors found", error);
    }

    [Fact]
    public void ValidExpressionsStillCompileWithoutErrors()
    {
        var validExpressions = new[]
        {
            "IsMatch(Name, '^[A-Z]')",
            "IndexOfMatch(Text, '\\d+')",
            "Contains(Name, 'test') ci",
            "Length(Items) > 5",
            "Round(Value, 2)",
            "TypeOf(Data) = 'String'",
            "Name like '%test%'",
            "StartsWith(Path, '/') and EndsWith(Path, '.json')"
        };
        
        foreach (var expr in validExpressions)
        {
            var result = SerilogExpression.TryCompile(expr, out var compiled, out var error);
            Assert.True(result, $"Failed to compile: {expr}. Error: {error}");
            Assert.NotNull(compiled);
            Assert.Null(error);
        }
    }

    [Fact]
    public void CompileMethodStillThrowsForInvalidExpressions()
    {
        // Ensure Compile() method (not TryCompile) maintains throwing behavior for invalid expressions
        Assert.Throws<ArgumentException>(() => 
            SerilogExpression.Compile("UnknownFunction()"));
        
        Assert.Throws<ArgumentException>(() => 
            SerilogExpression.Compile("IsMatch(Name, '[invalid')"));
        
        Assert.Throws<ArgumentException>(() => 
            SerilogExpression.Compile("Length(Name) ci"));
        
        Assert.Throws<ArgumentException>(() => 
            SerilogExpression.Compile("IndexOfMatch(Text, '(?<')"));
    }

    [Theory]
    [InlineData("IsMatch(Name, Name)")] // Non-constant pattern
    [InlineData("IndexOfMatch(Text, Value)")] // Non-constant pattern
    public void NonConstantRegexPatternsHandledGracefully(string expression)
    {
        // These should compile but may log warnings (not errors)
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        
        // These compile successfully but return undefined at runtime
        Assert.True(result);
        Assert.NotNull(compiled);
        Assert.Null(error);
    }

    [Fact]
    public void RegexTimeoutIsRespected()
    {
        // This regex should compile fine - timeout only matters at runtime
        var expression = @"IsMatch(Text, '(a+)+b')";
        
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        
        Assert.True(result);
        Assert.NotNull(compiled);
        Assert.Null(error);
    }

    [Fact]
    public void ComplexExpressionsWithMixedIssues()
    {
        var expression = "(UnknownFunc1() or IsMatch(Name, '(invalid')) and NotRealFunc() ci";
        var result = SerilogExpression.TryCompile(expression, out var compiled, out var error);
        
        Assert.False(result);
        Assert.Null(compiled);
        Assert.NotNull(error);
        
        // Should report multiple errors
        Assert.Contains("Multiple errors found", error);
        Assert.Contains("UnknownFunc1", error);
        Assert.Contains("NotRealFunc", error);
        Assert.Contains("Invalid regular expression", error);
    }
}