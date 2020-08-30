using System;
using System.Linq;
using Serilog.Events;

namespace Serilog.Expressions.Runtime
{
    static class Representation
    {
        static readonly Type[] NumericTypes = { typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double) };

        static readonly Type[] AllowedPrimitiveTypes = { typeof(string), typeof(bool), typeof(TimeSpan), typeof(DateTime),
            typeof(DateTimeOffset), typeof(decimal) };

        // Convert scalars into a small set of primitive types; leave everything else unchanged. This
        // makes it easier to flow values through operations.
        public static object Represent(LogEventPropertyValue value)
        {
            if (!(value is ScalarValue sv)) return value;
            
            if (sv.Value == null)
                return null;

            if (Array.IndexOf(AllowedPrimitiveTypes, sv.Value.GetType()) != -1)
                return sv.Value;

            if (Array.IndexOf(NumericTypes, sv.Value.GetType()) != -1)
                return Convert.ChangeType(sv.Value, typeof(decimal));

            return sv.Value.ToString();

        }

        public static object Expose(object internalValue)
        {
            if (internalValue is Undefined)
                return null;

            if (internalValue is ScalarValue)
                throw new InvalidOperationException("A `ScalarValue` should have been converted within the filtering function, but was returned as a result.");

            if (internalValue is SequenceValue sequence)
                return sequence.Elements.Select(ExposeOrRepresent).ToArray();

            if (internalValue is StructureValue structure)
            {
                var r = structure.Properties.ToDictionary(p => p.Name, p => ExposeOrRepresent(p.Value));
                if (structure.TypeTag != null)
                    r["$type"] = structure.TypeTag;
                return r;
            }

            if (internalValue is DictionaryValue dictionary)
            {
                return dictionary.Elements.ToDictionary(p => ExposeOrRepresent(p.Key), p => ExposeOrRepresent(p.Value));
            }

            return internalValue;
        }

        static object ExposeOrRepresent(object internalValue)
        {
            if (internalValue is Undefined)
                return null;

            if (internalValue is ScalarValue sv)
                return Represent(sv);

            return Expose(internalValue);
        }

        public static LogEventPropertyValue Recapture(object value)
        {
            return value is LogEventPropertyValue lepv ? lepv : new ScalarValue(value);
        }
    }
}
