using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Templates.Compilation;
using Serilog.Templates.Parsing;

namespace Serilog.Templates
{
    /// <summary>
    /// Formats <see cref="LogEvent"/>s into text using embedded expressions.
    /// </summary>
    public class OutputTemplate : ITextFormatter
    {
        readonly IFormatProvider _formatProvider;
        readonly CompiledTemplate _compiled;

        /// <summary>
        /// Construct an <see cref="OutputTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="formatProvider">Optionally, an <see cref="IFormatProvider"/> to use when formatting
        /// embedded values.</param>
        public OutputTemplate(string template, IFormatProvider formatProvider = null)
        {
            _formatProvider = formatProvider;
            var parsed = TemplateParser.Parse(template);
            _compiled = TemplateCompiler.Compile(parsed);
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            _compiled.Evaluate(logEvent, output, _formatProvider);
        }
    }
}
