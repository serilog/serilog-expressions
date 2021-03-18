using System;

namespace Serilog.Expressions.Ast
{
    class AmbientNameExpression : Expression
    {
        readonly bool _requiresEscape;

        public AmbientNameExpression(string Name, bool isBuiltIn)
        {
            PropertyName = Name ?? throw new ArgumentNullException(nameof(Name));
            IsBuiltIn = isBuiltIn;
            _requiresEscape = !SerilogExpression.IsValidIdentifier(Name);
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
