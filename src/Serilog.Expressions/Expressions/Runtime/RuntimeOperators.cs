using System;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Compilation.Linq;

// ReSharper disable ForCanBeConvertedToForeach, InvertIf, MemberCanBePrivate.Global, UnusedMember.Global

namespace Serilog.Expressions.Runtime
{
    static class RuntimeOperators
    {
        static readonly LogEventPropertyValue ConstantTrue = new ScalarValue(true),
                                              ConstantFalse = new ScalarValue(false);

        internal static LogEventPropertyValue ScalarBoolean(bool value)
        {
            return value ? ConstantTrue : ConstantFalse;
        }
        
        public static LogEventPropertyValue Add(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return new ScalarValue(l + r);

            return default;
        }

        public static LogEventPropertyValue Subtract(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return new ScalarValue(l - r);

            return default;
        }

        public static LogEventPropertyValue Multiply(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return new ScalarValue(l * r);

            return default;
        }

        public static LogEventPropertyValue Divide(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r) &&
                r != 0)
                return new ScalarValue(l / r);

            return default;
        }

        public static LogEventPropertyValue Modulo(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r) &&
                r != 0)
                return new ScalarValue(l % r);

            return default;
        }

        public static LogEventPropertyValue Power(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return new ScalarValue(Math.Pow((double)l, (double)r));

            return default;
        }

        public static LogEventPropertyValue And(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            throw new InvalidOperationException("Logical operators should be evaluated intrinsically.");
        }

        public static LogEventPropertyValue Or(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            throw new InvalidOperationException("Logical operators should be evaluated intrinsically.");
        }

        public static LogEventPropertyValue LessThanOrEqual(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return ScalarBoolean(l <= r);

            return default;
        }

        public static LogEventPropertyValue LessThan(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return ScalarBoolean(l < r);

            return default;
        }

        public static LogEventPropertyValue GreaterThan(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return ScalarBoolean(l > r);

            return default;
        }

        public static LogEventPropertyValue GreaterThanOrEqual(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return ScalarBoolean(l >= r);

            return default;
        }

        public static LogEventPropertyValue Equal(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            // Undefined values propagate through comparisons
            if (left == null || right == null)
                return null;
            
            return ScalarBoolean(UnboxedEqualHelper(left, right));
        }

        static bool UnboxedEqualHelper(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (left == null || right == null)
                throw new ArgumentException("Undefined values should short-circuit.");
            
            if (Coerce.Numeric(left, out var l) &&
                Coerce.Numeric(right, out var r))
                return l == r;
            
            if (Coerce.String(left, out var ls) &&
                Coerce.String(right, out var rs))
                return ls == rs;
            
            if (left is ScalarValue sl &&
                right is ScalarValue sr)
                return sl.Value?.Equals(sr.Value) ?? sr.Value == null;

            if (left is SequenceValue ql &&
                right is SequenceValue qr)
            {
                // Not in any way optimized :-)
                return ql.Elements.Count == qr.Elements.Count &&
                       ql.Elements.Zip(qr.Elements, UnboxedEqualHelper).All(eq => eq);
            }

            return false;
        }

        public static LogEventPropertyValue _Internal_In(LogEventPropertyValue item, LogEventPropertyValue collection)
        {
            if (item == null)
                return null;
            
            if (collection is SequenceValue arr)
            {
                for (var i = 0; i < arr.Elements.Count; ++i)
                {
                    var element = arr.Elements[i];
                    if (element != null && UnboxedEqualHelper(element, item))
                        return ConstantTrue;
                }

                return ConstantFalse;
            }

            return null;
        }

        public static LogEventPropertyValue NotEqual(LogEventPropertyValue left, LogEventPropertyValue right)
        {
            if (left == null || right == null)
                return null;
            
            return ScalarBoolean(!UnboxedEqualHelper(left, right));
        }

        public static LogEventPropertyValue Negate(LogEventPropertyValue operand)
        {
            if (Coerce.Numeric(operand, out var numeric))
                return new ScalarValue(-numeric);
            return null;
        }

        public static LogEventPropertyValue Round(LogEventPropertyValue value, LogEventPropertyValue places)
        {
            if (!Coerce.Numeric(value, out var v) ||
                !Coerce.Numeric(places, out var p) ||
                p < 0 ||
                p > 32) // Check my memory, here :D
            {
                return null;
            }

            return new ScalarValue(Math.Round(v, (int)p));
        }

        public static LogEventPropertyValue Not(LogEventPropertyValue operand)
        {
            if (operand is null)
                return ConstantTrue;

            return Coerce.Boolean(operand, out var b) ?
                ScalarBoolean(!b) :
                null;
        }

        public static LogEventPropertyValue _Internal_StrictNot(LogEventPropertyValue operand)
        {
            return Coerce.Boolean(operand, out var b) ?
                ScalarBoolean(!b) :
                null;
        }

        public static LogEventPropertyValue Contains(LogEventPropertyValue corpus, LogEventPropertyValue pattern)
        {
            if (!Coerce.String(corpus, out var ctx) ||
                !Coerce.String(pattern, out var ptx))
                return null;

            return ScalarBoolean(ctx.Contains(ptx));
        }

        public static LogEventPropertyValue IndexOf(LogEventPropertyValue corpus, LogEventPropertyValue pattern)
        {
            if (!Coerce.String(corpus, out var ctx) ||
                !Coerce.String(pattern, out var ptx))
                return null;

            return new ScalarValue(ctx.IndexOf(ptx, StringComparison.Ordinal));
        }

        public static LogEventPropertyValue LastIndexOf(LogEventPropertyValue corpus, LogEventPropertyValue pattern)
        {
            if (!Coerce.String(corpus, out var ctx) ||
                !Coerce.String(pattern, out var ptx))
                return null;

            return new ScalarValue(ctx.LastIndexOf(ptx, StringComparison.Ordinal));
        }

        public static LogEventPropertyValue Length(LogEventPropertyValue arg)
        {
            if (Coerce.String(arg, out var s))
                return new ScalarValue(s.Length);

            if (arg is SequenceValue seq)
                return new ScalarValue(seq.Elements.Count);

            return null;
        }

        public static LogEventPropertyValue StartsWith(LogEventPropertyValue corpus, LogEventPropertyValue pattern)
        {
            if (!Coerce.String(corpus, out var ctx) ||
                !Coerce.String(pattern, out var ptx))
                return null;

            return ScalarBoolean(ctx.StartsWith(ptx, StringComparison.Ordinal));
        }

        public static LogEventPropertyValue EndsWith(LogEventPropertyValue corpus, LogEventPropertyValue pattern)
        {
            if (!Coerce.String(corpus, out var ctx) ||
                !Coerce.String(pattern, out var ptx))
                return null;

            return ScalarBoolean(ctx.EndsWith(ptx, StringComparison.Ordinal));
        }

        public static LogEventPropertyValue Has(LogEventPropertyValue value)
        {
            return ScalarBoolean(value != null);
        }

        public static LogEventPropertyValue ElementAt(LogEventPropertyValue items, LogEventPropertyValue index)
        {
            if (items is SequenceValue arr && Coerce.Numeric(index, out var ix))
            {
                if (ix != Math.Floor(ix))
                    return null;

                var idx = (int)ix;
                if (idx >= arr.Elements.Count)
                    return null;

                return arr.Elements.ElementAt(idx);
            }
            
            if (items is StructureValue st && Coerce.String(index, out var s))
            {
                if (!LinqExpressionCompiler.TryGetStructurePropertyValue(st, s, out var value))
                    return null;

                return value;
            }
            
            if (items is DictionaryValue dict && index is ScalarValue sv)
            {
                // The lack of eager numeric type coercion means that here, `sv` may logically equal one
                // of the keys, but not be equal according to the dictionary's `IEqualityComparer`.
                var entry = dict.Elements.FirstOrDefault(kv => kv.Key != null && UnboxedEqualHelper(kv.Key, sv));
                return entry.Value; // KVP is a struct; default is a pair of nulls.
            }

            return null;
        }

        public static LogEventPropertyValue _Internal_Any(LogEventPropertyValue items, LogEventPropertyValue predicate)
        {
            if (!Coerce.Predicate(predicate, out var pred))
                return null;

            if (items is SequenceValue arr)
            {
                return ScalarBoolean(arr.Elements.Any(e => Coerce.IsTrue(pred(e))));
            }

            if (items is StructureValue structure)
            {
                return ScalarBoolean(structure.Properties.Any(e => Coerce.IsTrue(pred(e.Value))));
            }

            return null;
        }

        public static LogEventPropertyValue _Internal_All(LogEventPropertyValue items, LogEventPropertyValue predicate)
        {
            if (!Coerce.Predicate(predicate, out var pred))
                return null;

            if (items is SequenceValue arr)
            {
                return ScalarBoolean(arr.Elements.All(e => Coerce.IsTrue(pred(e))));
            }

            if (items is StructureValue structure)
            {
                return ScalarBoolean(structure.Properties.All(e => Coerce.IsTrue(pred(e.Value))));
            }

            return null;
        }

        public static LogEventPropertyValue TagOf(LogEventPropertyValue value)
        {
            if (value is StructureValue structure)
                return new ScalarValue(structure.TypeTag); // I.e. may be null

            return null;
        }

        public static LogEventPropertyValue TypeOf(LogEventPropertyValue value)
        {
            if (value is DictionaryValue)
                return new ScalarValue("dictionary");

            if (value is StructureValue)
                return new ScalarValue("structure");

            if (value is SequenceValue)
                return new ScalarValue("sequence");

            if (value is ScalarValue scalar)
            {
                return new ScalarValue(scalar.Value?.GetType().ToString() ?? "null");
            }

            return new ScalarValue("undefined");
        }

        public static LogEventPropertyValue _Internal_IsNull(LogEventPropertyValue value)
        {
            return ScalarBoolean(value is null || value is ScalarValue sv && sv.Value == null);
        }

        public static LogEventPropertyValue _Internal_IsNotNull(LogEventPropertyValue value)
        {
            return ScalarBoolean(!(value is null || value is ScalarValue sv && sv.Value == null));
        }

        public static LogEventPropertyValue Coalesce(LogEventPropertyValue v1, LogEventPropertyValue v2)
        {
            if (v1 is null || v1 is ScalarValue sv && sv.Value == null)
                return v2;

            return v1;
        }

        public static LogEventPropertyValue Substring(LogEventPropertyValue sval, LogEventPropertyValue startIndex, LogEventPropertyValue length)
        {
            if (!Coerce.String(sval, out var str) ||
                !Coerce.Numeric(startIndex, out var si))
                return null;
            
            if (si < 0 || si >= str.Length || (int)si != si)
                return null;

            if (length == null)
                return new ScalarValue(str.Substring((int)si));

            if (!Coerce.Numeric(length, out var len) || (int)len != len)
                return null;

            if (len + si > str.Length)
                return new ScalarValue(str.Substring((int)si));

            return new ScalarValue(str.Substring((int)si, (int)len));
        }

        public static LogEventPropertyValue IndexOfMatch(LogEventPropertyValue corpus, LogEventPropertyValue regex)
        {
            throw new InvalidOperationException("Regular expression evaluation is intrinsic.");
        }

        public static LogEventPropertyValue IsMatch(LogEventPropertyValue corpus, LogEventPropertyValue regex)
        {
            throw new InvalidOperationException("Regular expression evaluation is intrinsic.");
        }
    }
}
