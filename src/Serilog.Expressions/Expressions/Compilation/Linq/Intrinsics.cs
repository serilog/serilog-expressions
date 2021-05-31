// Copyright © Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Expressions.Runtime;
using Serilog.Formatting.Display;
using Serilog.Parsing;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace Serilog.Expressions.Compilation.Linq
{
    class Intrinsics
    {
        static readonly LogEventPropertyValue NegativeOne = new ScalarValue(-1);
        static readonly LogEventPropertyValue Tombstone = new ScalarValue("😬 (if you see this you have found a bug.)");
        
        // TODO #19: formatting is culture-specific.
        static readonly MessageTemplateTextFormatter MessageFormatter = new("{Message:lj}");

        public static List<LogEventPropertyValue?> CollectSequenceElements(LogEventPropertyValue?[] elements)
        {
            return elements.ToList();
        }

        public static List<LogEventPropertyValue?> ExtendSequenceValueWithItem(List<LogEventPropertyValue?> elements,
            LogEventPropertyValue? element)
        {
            // Mutates the list; returned so we can nest calls instead of emitting a block.
            if (element != null)
                elements.Add(element);
            return elements;
        }

        public static List<LogEventPropertyValue?> ExtendSequenceValueWithSpread(List<LogEventPropertyValue?> elements,
            LogEventPropertyValue? content)
        {
            if (content is SequenceValue sequence)
                foreach (var element in sequence.Elements)
                    elements.Add(element);
            
            return elements;
        }

        public static LogEventPropertyValue ConstructSequenceValue(List<LogEventPropertyValue?> elements)
        {
            if (elements.Any(el => el == null))
                return new SequenceValue(elements.Where(el => el != null));
            
            return new SequenceValue(elements);
        }
        
        public static List<LogEventProperty> CollectStructureProperties(string[] names, LogEventPropertyValue?[] values)
        {
            var properties = new List<LogEventProperty>();
            for (var i = 0; i < names.Length; ++i)
            {
                var name = names[i];
                var value = values[i];
                properties.Add(new LogEventProperty(name, value ?? Tombstone));
            }

            return properties;
        }

        public static LogEventPropertyValue ConstructStructureValue(List<LogEventProperty> properties)
        {
            if (properties.Any(p => p == null || p.Value == Tombstone))
                return new StructureValue(properties.Where(p => p != null && p.Value != Tombstone));

            return new StructureValue(properties);
        }
        
        public static List<LogEventProperty> ExtendStructureValueWithSpread(
            List<LogEventProperty> properties,
            LogEventPropertyValue? content)
        {
            if (content is StructureValue structure)
            {
                foreach (var property in structure.Properties)
                    if (property != null)
                        properties.Add(property);
            }

            return properties;
        }
        
        public static List<LogEventProperty> ExtendStructureValueWithProperty(
            List<LogEventProperty> properties,
            string name,
            LogEventPropertyValue? value)
        {
            // Mutates the list; returned so we can nest calls instead of emitting a block.
            properties.Add(new LogEventProperty(name, value ?? Tombstone));
            return properties;
        }

        public static LogEventPropertyValue CompleteStructureValue(List<LogEventProperty> properties)
        {
            var result = new OrderedDictionary();
            foreach (var property in properties)
            {
                if (result.Contains(property.Name))
                    result.Remove(property.Name);
                if (property.Value != Tombstone)
                    result.Add(property.Name, new LogEventProperty(property.Name, property.Value));
            }
            return new StructureValue(result.Values.Cast<LogEventProperty>().ToList());
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

        public static LogEventPropertyValue? GetPropertyValue(EvaluationContext ctx, string propertyName)
        {
            if (!ctx.LogEvent.Properties.TryGetValue(propertyName, out var value))
                return null;

            return value;
        }

        public static LogEventPropertyValue? GetLocalValue(EvaluationContext ctx, string localName)
        {
            if (!Locals.TryGetValue(ctx.Locals, localName, out var value))
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

        public static LogEventPropertyValue? GetRenderings(LogEvent logEvent)
        {
            List<LogEventPropertyValue>? elements = null;
            foreach (var token in logEvent.MessageTemplate.Tokens)
            {
                if (token is PropertyToken {Format: { }} pt)
                {
                    elements ??= new List<LogEventPropertyValue>();
                    
                    var space = new StringWriter();
                  
                    // TODO #19: formatting is culture-specific.
                    pt.Render(logEvent.Properties, space);
                    elements.Add(new ScalarValue(space.ToString()));
                }
            }

            return elements == null ? null : new SequenceValue(elements);
        }
    }
}