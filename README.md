# _Serilog.Expressions_

An embeddable mini-language for filtering, enriching, and formatting Serilog 
events, ideal for use in JSON or XML configuration.

## Getting started

Install the package from NuGet:

```shell
dotnet add package Serilog.Expressions
```

The package adds extension methods to Serilog's `Filter` and 
`Enrich` configuration objects, along with an `OutputTemplate`
type that's compatible with Serilog sinks accepting an
`ITextFormatter`.

### Filtering

_Serilog.Expressions_ adds `ByExcluding()` and `ByIncludingOnly()` 
overloads to the `Filter` configuration object that accept filter
expressions:

```csharp
Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding("RequestPath like '/health%'")
    .CreateLogger();
```

Events with a `RequestPath` property that matches the expression
will be excluded by the filter.

> Note that if the expression syntax is invalid, an `ArgumentException` will
be thrown from the `ByExcluding()` method, and by similar methods elsewhere
in the package. To check expression syntax without throwing, see the
`Try*()` methods in the `SerilogExpression` class.

#### An `appSettings.json` JSON configuration example

In [`appSettings.json` 
configuration](https://github.com/serilog/serilog-settings-configuration) 
this is written as:

```json
{
  "Serilog": {
    "Using": ["Serilog.Expressions"],
    "Filter": [
      {
        "Name": "ByExcluding",
        "Args": {
          "expression": "RequestPath like '/health%'"
        }
      }
    ]
  }
}
```

#### An `<appSettings>` XML configuration example

In [XML configuration files](https://github.com/serilog/serilog-settings-appsettings),
this is written as:

```xml
  <appSettings>
    <add key="serilog:using:Expressions" value="Serilog.Expressions" />
    <add key="serilog:filter:ByExcluding.expression" value="RequestPath like '/health%'" />
  </appSettings>
``` 

### Enriching

### Formatting

## Language reference

## Working with the raw API

The package provides the class `SerilogExpression` in the `Serilog.Expressions` namespace
for working with expressions.

```csharp
if (SerilogExpression.TryCompile("RequestPath like '/health%'", out var compiled, out var error)
{
    // `compiled` is a function that can be executed against `LogEvent`s:
    var result = compiled(someEvent);

    // `result` will contain a `LogEventPropertyValue`, or `null` if the result of evaluating the
    // expression is undefined (for example if the event has no `RequestPath` property).
    if (result is ScalarValue value &&
        value.Value is bool matches &&
        matches)
    {
        Console.WriteLine("The event matched.");
    }
}
else
{
    // `error` describes a syntax error.
    Console.WriteLine($"Couldn't compile the expression; {error}.");
}
```

Compiled expression delegates return `LogEventPropertyValue` because this is the most
convenient type to work with in many Serilog scenarios (enrichers, sinks, ...). To
convert the result to plain-old-.NET-types like `string`, `bool`, `Dictionary<K,V>` and
 `Array`, use the functions in the `Serilog.Expressions.ExpressionResult` class:
 
```csharp
    var result = compiled(someEvent);

    // `true` only if `result` is a scalar Boolean `true`; `false` otherwise:
    if (ExpressionResult.IsTrue(result))
    {
        Console.WriteLine("The event matched.");
    }
```
