using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Expressions.Runtime;
using Serilog.Formatting.Display;

namespace Serilog.Expressions.Compilation.Linq
{
    static class Intrinsics
    {
        static readonly LogEventPropertyValue NegativeOne = new ScalarValue(-1);
        static readonly MessageTemplateTextFormatter MessageFormatter = new MessageTemplateTextFormatter("{Message:lj}");

        public static LogEventPropertyValue? ConstructSequenceValue(LogEventPropertyValue?[] elements)
        {
            // Avoid upsetting Serilog's (currently) fragile `SequenceValue.Render()`.
            if (elements.Any(el => el == null))
                return null;
            return new SequenceValue(elements);
        }
        
        public static LogEventPropertyValue? ConstructStructureValue(string[] names, LogEventPropertyValue?[] values)
        {
            var properties = new List<LogEventProperty>();
            for (var i = 0; i < names.Length; ++i)
            {
                var value = values[i];
                
                // Avoid upsetting Serilog's `Structure.Render()`.
                if (value == null) return null;
                
                properties.Add(new LogEventProperty(names[i], value));
            }
            return new StructureValue(properties);
        }
        
        public static bool CoerceToScalarBoolean(LogEventPropertyValue value)
        {
            if (value is ScalarValue sv && sv.Value is bool b)
                return b;
            return false;
        }
        
        public static LogEventPropertyValue? IndexOfMatch(LogEventPropertyValue value, Regex regex)
        {
            if (value is ScalarValue scalar &&
                scalar.Value is string s)
            {
                var m = regex.Match(s);
                if (m.Success)
                    return new ScalarValue(m.Index);
                return NegativeOne;
            }

            return null;
        }

        public static LogEventPropertyValue? GetPropertyValue(LogEvent context, string propertyName)
        {
            if (!context.Properties.TryGetValue(propertyName, out var value))
                return null;

            return value;
        }

        public static LogEventPropertyValue? TryGetStructurePropertyValue(StringComparison sc, LogEventPropertyValue maybeStructure, string name)
        {
            if (maybeStructure is StructureValue sv)
            {
                foreach (var prop in sv.Properties)
                {
                    if (prop.Name.Equals(name, sc))
                    {
                        return prop.Value;
                    }
                }
            }

            return null;
        }

        public static string RenderMessage(LogEvent logEvent)
        {
            // Use the same `:lj`-style formatting default as Serilog.Sinks.Console.
            var sw = new StringWriter();
            MessageFormatter.Format(logEvent, sw);
            return sw.ToString();
        }
    }
}