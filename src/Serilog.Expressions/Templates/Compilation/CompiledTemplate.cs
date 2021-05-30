using System.IO;
using Serilog.Expressions;

namespace Serilog.Templates.Compilation
{
    abstract class CompiledTemplate
    {
        public abstract void Evaluate(EvaluationContext ctx, TextWriter output);
    }
}
