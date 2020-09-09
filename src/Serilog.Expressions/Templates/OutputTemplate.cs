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
        /// <summary>
        /// Construct an <see cref="OutputTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="formatProvider">Optionally, an <see cref="IFormatProvider"/> to use when formatting
        /// embedded values.</param>
        /// <param name="result">The parsed template, if successful.</param>
        /// <param name="error">A description of the error, if unsuccessful.</param>
        /// <returns><c langword="true">true</c> if the template was well-formed.</returns>
        public static bool TryParse(
            string template,
            IFormatProvider formatProvider,
            out OutputTemplate result,
            out string error)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            
            if (!TemplateParser.TryParse(template, out var parsed, out error))
            {
                result = null;
                return false;
            }

            result = new OutputTemplate(TemplateCompiler.Compile(parsed), formatProvider);
            return true;
        }
        
        readonly IFormatProvider _formatProvider;
        readonly CompiledTemplate _compiled;

        OutputTemplate(CompiledTemplate compiled, IFormatProvider formatProvider)
        {
            _compiled = compiled;
            _formatProvider = formatProvider;
        }

        /// <summary>
        /// Construct an <see cref="OutputTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="formatProvider">Optionally, an <see cref="IFormatProvider"/> to use when formatting
        /// embedded values.</param>
        public OutputTemplate(string template, IFormatProvider formatProvider = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            if (!TemplateParser.TryParse(template, out var parsed, out var error))
                throw new ArgumentException(error);
            
            _compiled = TemplateCompiler.Compile(parsed);
            _formatProvider = formatProvider;
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            _compiled.Evaluate(logEvent, output, _formatProvider);
        }
    }
}
