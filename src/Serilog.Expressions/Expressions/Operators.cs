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
        public const string OpNotEqual = "NotEqual";
        public const string OpNegate = "Negate";
        public const string OpNot = "Not";
        public const string OpContains = "Contains";
        public const string OpIndexOf = "IndexOf";
        public const string OpLastIndexOf = "IndexOf";
        public const string OpLength = "Length";
        public const string OpStartsWith = "StartsWith";
        public const string OpEndsWith = "EndsWith";
        public const string OpHas = "Has";
        public const string OpDateTime = "DateTime";
        public const string OpTimeSpan = "TimeSpan";
        public const string OpTimeOfDay = "TimeOfDay";
        public const string OpElementAt = "ElementAt";
        public const string RuntimeOpAny = "_Internal_Any";
        public const string RuntimeOpAll = "_Internal_All";
        public const string OpTypeOf = "TypeOf";
        public const string OpTagOf = "TagOf";
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
        public const string OpIndexOfMatch = "IndexOfMatch";
        public const string OpIsMatch = "IsMatch";

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

        public static bool SameOperator(string op1, string op2)
        {
            if (op1 == null) throw new ArgumentNullException(nameof(op1));
            if (op2 == null) throw new ArgumentNullException(nameof(op2));

            return OperatorComparer.Equals(op1, op2);
        }
        
        public static string ToRuntimeWildcardOperator(IndexerWildcard wildcard)
        {
            return "_Internal_" + wildcard; // "Any"/"All"
        }
    }
}
