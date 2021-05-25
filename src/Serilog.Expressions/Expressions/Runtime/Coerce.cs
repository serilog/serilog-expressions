using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Serilog.Events;

namespace Serilog.Expressions.Runtime
{
    static class Coerce
    {
        static readonly Type[] NumericTypes = { typeof(decimal),
            typeof(int), typeof(long), typeof(double), 
            typeof(float), typeof(uint), typeof(sbyte), 
            typeof(byte), typeof(short), typeof(ushort), typeof(ulong) };

        public static bool Numeric(LogEventPropertyValue? value, out decimal numeric)
        {
            if (value is ScalarValue sv &&
                sv.Value != null &&
                NumericTypes.Contains(sv.Value.GetType()))
            {
                numeric = (decimal)Convert.ChangeType(sv.Value, typeof(decimal));
                return true;
            }

            numeric = default;
            return false;
        }

        public static bool Boolean(LogEventPropertyValue? value, out bool boolean)
        {
            if (value is ScalarValue sv &&
                sv.Value is bool b)
            {
                boolean = b;
                return true;
            }

            boolean = default;
            return false;
        }

        public static bool IsTrue(LogEventPropertyValue? value)
        {
            return Boolean(value, out var b) && b;
        }

        public static bool String(LogEventPropertyValue? value, [MaybeNullWhen(false)] out string str)
        {
            if (value is ScalarValue sv)
            {
                if (sv.Value is string s)
                {
                    str = s;
                    return true;
                }
                
                if (sv.Value is Exception ex)
                {
                    str = ex.ToString();
                    return true;
                }

                if (sv.Value?.GetType().IsEnum ?? false)
                {
                    str = sv.Value.ToString()!;
                    return true;
                }
            }

            str = default;
            return false;
        }

        public static bool Predicate(LogEventPropertyValue? value,
            [MaybeNullWhen(false)] out Func<LogEventPropertyValue, LogEventPropertyValue> predicate)
        {
            if (value is ScalarValue sv &&
                sv.Value is Func<LogEventPropertyValue, LogEventPropertyValue> pred)
            {
                predicate = pred;
                return true;
            }

            predicate = default;
            return false;
        }
    }
}