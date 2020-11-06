using System;
using System.IO;
using Serilog.Events;

namespace Serilog.Templates.Compilation
{
    class CompiledTemplateBlock : CompiledTemplate
    {
        readonly CompiledTemplate[] _elements;

        public CompiledTemplateBlock(CompiledTemplate[] elements)
        {
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }
        
        public override void Evaluate(LogEvent logEvent, TextWriter output, IFormatProvider? formatProvider)
        {
            foreach (var element in _elements)
                element.Evaluate(logEvent, output, formatProvider);
        }
    }
}