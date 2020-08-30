using System;

namespace Serilog.Expressions.Ast
{
    class AmbientPropertyExpression : Expression
    {
        readonly bool _requiresEscape;

        public AmbientPropertyExpression(string propertyName, bool isBuiltIn)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            IsBuiltIn = isBuiltIn;
            _requiresEscape = !SerilogExpression.IsValidPropertyName(propertyName);
        }

        public string PropertyName { get; }

        public bool IsBuiltIn { get; }

        public override string ToString()
        {
            if (_requiresEscape)
                return $"@Properties['{SerilogExpression.EscapeStringContent(PropertyName)}']";

            return (IsBuiltIn ? "@" : "") + PropertyName;
        }
    }
}