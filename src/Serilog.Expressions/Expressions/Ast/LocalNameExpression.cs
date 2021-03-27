using System;

namespace Serilog.Expressions.Ast
{
    class LocalNameExpression : Expression
    {
        public LocalNameExpression(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public override string ToString()
        {
            // No unambiguous syntax for this right now, `$` will do to make these stand out when debugging,
            // but the result won't round-trip parse.
            return $"${Name}";
        }
    }
}
