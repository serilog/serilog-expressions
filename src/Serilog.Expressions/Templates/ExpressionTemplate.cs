using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Expressions.Compilation;
using Serilog.Formatting;
using Serilog.Templates.Compilation;
using Serilog.Templates.Parsing;

namespace Serilog.Templates
{
    /// <summary>
    /// Formats <see cref="LogEvent"/>s into text using embedded expressions.
    /// </summary>
    public class ExpressionTemplate : ITextFormatter
    {
        readonly IFormatProvider? _formatProvider;
        readonly CompiledTemplate _compiled;

        /// <summary>
        /// Construct an <see cref="ExpressionTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="result">The parsed template, if successful.</param>
        /// <param name="error">A description of the error, if unsuccessful.</param>
        /// <returns><c langword="true">true</c> if the template was well-formed.</returns>
        public static bool TryParse(
            string template,
            [MaybeNullWhen(false)] out ExpressionTemplate result,
            [MaybeNullWhen(true)] out string error)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return TryParse(template, null, null, out result, out error);
        }

        /// <summary>
        /// Construct an <see cref="ExpressionTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="formatProvider">Optionally, an <see cref="IFormatProvider"/> to use when formatting
        /// embedded values.</param>
        /// <param name="result">The parsed template, if successful.</param>
        /// <param name="error">A description of the error, if unsuccessful.</param>
        /// <param name="nameResolver">Optionally, a <see cref="NameResolver"/>
        /// with which to resolve function names that appear in the template.</param>
        /// <returns><c langword="true">true</c> if the template was well-formed.</returns>
        public static bool TryParse(
            string template,
            IFormatProvider? formatProvider,
            NameResolver? nameResolver,
            [MaybeNullWhen(false)] out ExpressionTemplate result,
            [MaybeNullWhen(true)] out string error)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var templateParser = new TemplateParser();
            if (!templateParser.TryParse(template, out var parsed, out error))
            {
                result = null;
                return false;
            }

            result = new ExpressionTemplate(TemplateCompiler.Compile(parsed, DefaultFunctionNameResolver.Build(nameResolver)), formatProvider);
            return true;
        }
        
        ExpressionTemplate(CompiledTemplate compiled, IFormatProvider? formatProvider)
        {
            _compiled = compiled;
            _formatProvider = formatProvider;
        }

        /// <summary>
        /// Construct an <see cref="ExpressionTemplate"/>.
        /// </summary>
        /// <param name="template">The template text.</param>
        /// <param name="formatProvider">Optionally, an <see cref="IFormatProvider"/> to use when formatting
        /// embedded values.</param>
        /// <param name="nameResolver">Optionally, a <see cref="NameResolver"/>
        /// with which to resolve function names that appear in the template.</param>
        public ExpressionTemplate(
            string template,
            IFormatProvider? formatProvider = null,
            NameResolver? nameResolver = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var templateParser = new TemplateParser();
            if (!templateParser.TryParse(template, out var parsed, out var error))
                throw new ArgumentException(error);
            
            _compiled = TemplateCompiler.Compile(parsed, DefaultFunctionNameResolver.Build(nameResolver));
            _formatProvider = formatProvider;
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            _compiled.Evaluate(new EvaluationContext(logEvent), output, _formatProvider);
        }
    }
}
