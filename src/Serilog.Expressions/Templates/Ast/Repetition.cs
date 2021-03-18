using System;
using Serilog.Expressions.Ast;

namespace Serilog.Templates.Ast
{
    class Repetition: Template
    {
        public Expression Enumerable { get; }
        public string[] BindingNames { get; }
        public Template Body { get; }
        public Template? Delimiter { get; }
        public Template? Alternative { get; }

        public Repetition(
            Expression enumerable,
            string[] bindingNames,
            Template body,
            Template? delimiter,
            Template? alternative)
        {
            Enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            BindingNames = bindingNames;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Delimiter = delimiter;
            Alternative = alternative;
        }
    }
}
