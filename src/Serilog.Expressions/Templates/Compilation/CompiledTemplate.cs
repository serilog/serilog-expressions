using System;
using System.IO;
using Serilog.Events;

namespace Serilog.Templates.Compilation
{
    abstract class CompiledTemplate
    {
        public abstract void Evaluate(LogEvent logEvent, TextWriter output, IFormatProvider? formatProvider);
    }
}