# _Serilog Expressions_

An embeddable mini-language for filtering, enriching, and formatting Serilog 
events, ideal for use with JSON or XML configuration.

## Getting started

Install the package from NuGet:

```shell
dotnet add package Serilog.Expressions
```

The package adds extension methods to Serilog's `Filter`, `WriteTo`, and 
`Enrich` configuration objects, along with an `OutputTemplate`
type that's compatible with Serilog sinks accepting an
`ITextFormatter`.

### Filtering example

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

## Supported configuration APIs

_Serilog.Expressions_ adds a number of expression-based overloads and helper methods to the Serilog configuration syntax:

 * `Filter.ByExcluding()`, `Filter.ByIncludingOnly()` - use an expression to filter events passing through the Serilog pipeline
 * `WriteTo.Conditional()` - use an expression to select the events passed to a particular sink
 * `Enrich.When()` - conditionally enable an enricher when events match an expression
 * `Enrich.WithComputed()` - add or modify event properties using an expression

## Formatting

_Serilog.Expressions_ includes the `OutputTemplate` class for text formatting. `OutputTemplate` implements `ITextFormatter`, so
it works with any text-based Serilog sink:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new OutputTemplate(
        "[{@t:HH:mm:ss} {@l:u3} ({SourceContext})] {@m} (first item is {Items[0]})\n{@x}"))
    .CreateLogger();
```

Note the use of `{Items[0]}`: "holes" in expression-based output templates can include arbitrary expressions.

## Language reference

### Built-in properties

The following properties are available in expressions:

 * All first-class properties of the event; no special syntax: `SourceContext` and `Items` are used in the formatting example above
 * `@t` - the event's timestamp, as a `DateTimeOffset`
 * `@m` - the rendered message
 * `@mt` - the raw message template
 * `@l` - the event's level, as a `LogEventLevel`
 * `@x` - the exception associated with the event, if any, as an `Exception`
 * `@p` - a dictionary containing all first-class properties; this supports properties with non-identifier names, for example `@p['snake-case-name']`

### Data types

### Functions

### String manipulation

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
