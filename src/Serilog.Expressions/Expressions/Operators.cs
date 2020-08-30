using System;
using System.Collections.Generic;
using Serilog.Expressions.Ast;

namespace Serilog.Expressions
{
    static class Operators
    {
        static StringComparer OperatorComparer { get; } = StringComparer.OrdinalIgnoreCase;

        // Core filter language
        // Op* means usable in expressions _and_ runtime executable.
        // RuntimeOp* means runtime only.

        public const string OpAdd = "Add";
        public const string OpSubtract = "Subtract";
        public const string OpMultiply = "Multiply";
        public const string OpDivide = "Divide";
        public const string OpModulo = "Modulo";
        public const string OpPower = "Power";
        public const string OpRound = "Round";
        public const string OpAnd = "And";
        public const string OpOr = "Or";
        public const string OpLessThanOrEqual = "LessThanOrEqual";
        public const string OpLessThan = "LessThan";
        public const string OpGreaterThan = "GreaterThan";
        public const string OpGreaterThanOrEqual = "GreaterThanOrEqual";
        public const string OpEqual = "Equal";
        public const string RuntimeOpEqualIgnoreCase = "_Internal_EqualIgnoreCase";
        public const string RuntimeOpEqualPattern = "_Internal_EqualPattern";
        public const string OpNotEqual = "NotEqual";
        public const string RuntimeOpNotEqualIgnoreCase = "_Internal_NotEqualIgnoreCase";
        public const string RuntimeOpNotEqualPattern = "_Internal_NotEqualPattern";
        public const string OpNegate = "Negate";
        public const string OpNot = "Not";
        public const string OpContains = "Contains";
        public const string RuntimeOpContainsIgnoreCase = "_Internal_ContainsIgnoreCase";
        public const string RuntimeOpContainsPattern = "_Internal_ContainsPattern";
        public const string OpIndexOf = "IndexOf";
        public const string RuntimeOpIndexOfIgnoreCase = "_Internal_IndexOfIgnoreCase";
        public const string RuntimeOpIndexOfPattern = "_Internal_IndexOfPattern";
        public const string OpLength = "Length";
        public const string OpStartsWith = "StartsWith";
        public const string RuntimeOpStartsWithIgnoreCase = "_Internal_StartsWithIgnoreCase";
        public const string RuntimeOpStartsWithPattern = "_Internal_StartsWithPattern";
        public const string OpEndsWith = "EndsWith";
        public const string RuntimeOpEndsWithIgnoreCase = "_Internal_EndsWithIgnoreCase";
        public const string RuntimeOpEndsWithPattern = "_Internal_EndsWithPattern";
        public const string OpHas = "Has";
        public const string OpArrived = "Arrived";
        public const string OpDateTime = "DateTime";
        public const string OpTimeSpan = "TimeSpan";
        public const string OpTimeOfDay = "TimeOfDay";
        public const string OpElementAt = "ElementAt";
        public const string RuntimeOpAny = "_Internal_Any";
        public const string RuntimeOpAll = "_Internal_All";
        public const string OpTypeOf = "TypeOf";
        public const string OpTotalMilliseconds = "TotalMilliseconds";
        public const string RuntimeOpIsNull = "_Internal_IsNull";
        public const string RuntimeOpIsNotNull = "_Internal_IsNotNull";
        public const string OpCoalesce = "Coalesce";
        public const string IntermediateOpSqlLike = "_Internal_Like";
        public const string IntermediateOpSqlNotLike = "_Internal_NotLike";
        public const string IntermediateOpSqlIs = "_Internal_Is";
        public const string RuntimeOpSqlIn = "_Internal_In";
        public const string IntermediateOpSqlNotIn = "_Internal_NotIn";
        public const string RuntimeOpStrictNot = "_Internal_StrictNot";
        public const string OpSubstring = "Substring";
        public const string RuntimeOpNewSequence = "_Internal_NewSequence";

        // Breaks the symmetry because there's no other way to express this in SQL.
        public const string OpIndexOfIgnoreCase = "IndexOfIgnoreCase";

        public static readonly HashSet<string> WildcardComparators = new HashSet<string>(OperatorComparer)
        {
            OpContains,
            OpStartsWith,
            OpEndsWith,
            OpNotEqual,
            OpEqual,
            OpLessThan,
            OpLessThanOrEqual,
            OpGreaterThan,
            OpGreaterThanOrEqual,
            IntermediateOpSqlLike,
            IntermediateOpSqlNotLike,
            RuntimeOpSqlIn,
            IntermediateOpSqlNotIn,
            IntermediateOpSqlIs
        };

        public static readonly HashSet<string> LogicalOperators = new HashSet<string>(OperatorComparer)
        {
            OpAnd,
            OpOr,
            OpNot
        };

        public static bool SameOperator(string op1, string op2)
        {
            if (op1 == null) throw new ArgumentNullException(nameof(op1));
            if (op2 == null) throw new ArgumentNullException(nameof(op2));

            return OperatorComparer.Equals(op1, op2);
        }

        public static string ToRuntimeIgnoreCase(string op)
        {
            return $"_Internal_{op}IgnoreCase";
        }

        public static string ToRuntimePattern(string op)
        {
            return $"_Internal_{op}Pattern";
        }

        public static string ToRuntimeWildcardOperator(IndexerWildcard wildcard)
        {
            return "_Internal_" + wildcard; // "Any"/"All"
        }

        public static bool IsRuntimeVariant(string op, string variant)
        {
            return SameOperator(op, variant) ||
                SameOperator(ToRuntimeIgnoreCase(op), variant) ||
                SameOperator(ToRuntimePattern(op), variant);
        }
    }
}
