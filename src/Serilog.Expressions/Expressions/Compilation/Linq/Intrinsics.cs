using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Expressions.Runtime;

namespace Serilog.Expressions.Compilation.Linq
{
    static class Intrinsics
    {
        static readonly LogEventPropertyValue NegativeOne = new ScalarValue(-1);
        
        public static LogEventPropertyValue ConstructSequenceValue(LogEventPropertyValue[] elements)
        {
            // Avoid upsetting Serilog's (currently) fragile `SequenceValue.Render()`.
            if (elements.Any(el => el == null))
                return null;
            return new SequenceValue(elements);
        }
        
        public static bool CoerceToScalarBoolean(LogEventPropertyValue value)
        {
            if (value is ScalarValue sv && sv.Value is bool b)
                return b;
            return false;
        }
        
        public static LogEventPropertyValue IndexOfMatch(LogEventPropertyValue value, Regex regex)
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

        public static LogEventPropertyValue GetPropertyValue(LogEvent context, string propertyName)
        {
            if (!context.Properties.TryGetValue(propertyName, out var value))
                return null;

            return value;
        }

        public static LogEventPropertyValue TryGetStructurePropertyValue(LogEventPropertyValue maybeStructure, string name)
        {
            if (maybeStructure is StructureValue sv)
            {
                foreach (var prop in sv.Properties)
                {
                    if (prop.Name == name)
                    {
                        return prop.Value;
                    }
                }
            }

            return null;
        }
    }
}