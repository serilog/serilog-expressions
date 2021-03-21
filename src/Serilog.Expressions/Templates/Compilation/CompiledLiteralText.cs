using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;

namespace Serilog.Templates.Compilation
{
    class CompiledLiteralText : CompiledTemplate
    {
        readonly string _text;

        public CompiledLiteralText(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider)
        {
            output.Write(_text);
        }
    }
}