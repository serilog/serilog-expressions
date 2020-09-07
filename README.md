# _Serilog.Expressions_

A mini-language for filtering, enriching, and formatting Serilog 
events, ideal for embedding in JSON or XML configuration.

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

### Enriching

### Formatting

## Language reference


