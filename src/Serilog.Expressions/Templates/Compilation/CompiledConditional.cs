using System;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;

namespace Serilog.Templates.Compilation
{
    class CompiledConditional : CompiledTemplate
    {
        readonly Evaluatable _condition;
        readonly CompiledTemplate _consequent;
        readonly CompiledTemplate? _alternative;

        public CompiledConditional(Evaluatable condition, CompiledTemplate consequent, CompiledTemplate? alternative)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
            _alternative = alternative;
        }

        public override void Evaluate(EvaluationContext ctx, TextWriter output, IFormatProvider? formatProvider)
        {
            if (ExpressionResult.IsTrue(_condition.Invoke(ctx)))
                _consequent.Evaluate(ctx, output, formatProvider);
            else
                _alternative?.Evaluate(ctx, output, formatProvider);
        }
    }
}
