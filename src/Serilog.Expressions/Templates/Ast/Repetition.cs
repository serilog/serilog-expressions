using System;
using Serilog.Expressions.Ast;

namespace Serilog.Templates.Ast
{
    class Repetition: Template
    {
        public Expression Enumerable { get; }
        public string? KeyOrElementName { get; }
        public string? ValueName { get; }
        public Template Body { get; }
        public Template? Separator { get; }
        public Template? Alternative { get; }

        public Repetition(
            Expression enumerable,
            string? keyOrElementName,
            string? valueName,
            Template body,
            Template? separator,
            Template? alternative)
        {
            Enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            KeyOrElementName = keyOrElementName;
            ValueName = valueName;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Separator = separator;
            Alternative = alternative;
        }
    }
}
