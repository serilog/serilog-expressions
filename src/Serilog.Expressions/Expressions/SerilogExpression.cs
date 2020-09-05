using System;
using System.Linq;
using Serilog.Expressions.Compilation;
using Serilog.Expressions.Parsing;

// ReSharper disable MemberCanBePrivate.Global

namespace Serilog.Expressions
{
    /// <summary>
    /// Helper methods to assist with construction of well-formed expressions.
    /// </summary>
    public static class SerilogExpression
    {
        /// <summary>
        /// Create an evaluation function based on the provided expression.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <returns>A function that evaluates the expression in the context of a log event.</returns>
        public static CompiledExpression Compile(string expression)
        {
            if (!TryCompile(expression, out var filter, out var error))
                throw new ArgumentException(error);

            return filter;
        }

        /// <summary>
        /// Create an evaluation function based on the provided expression.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <param name="result">A function that evaluates the expression in the context of a log event.</param>
        /// <param name="error">The reported error, if compilation was unsuccessful.</param>
        /// <returns>True if the function could be created; otherwise, false.</returns>
        public static bool TryCompile(string expression, out CompiledExpression result, out string error)
        {
            if (!ExpressionParser.TryParse(expression, out var root, out error))
            {
                result = null;
                return false;
            }

            result = ExpressionCompiler.Compile(root);
            error = null;
            return true;
        }

        /// <summary>
        /// Escape a value that is to appear in a `like` expression.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with any special values escaped. Will need to be passed through
        /// <see cref="EscapeStringContent(string)"/> if it is being embedded directly into a filter expression.</returns>
        // ReSharper disable once UnusedMember.Global
        public static string EscapeLikeExpressionContent(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return EscapeStringContent(text)
                .Replace("%", "%%")
                .Replace("_", "__");
        }

        /// <summary>
        /// Escape a fragment of text that will appear within a string.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with any special values escaped.</returns>
        public static string EscapeStringContent(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return text.Replace("'", "''");
        }

        /// <summary>
        /// Determine if the specified text is a valid property name.
        /// </summary>
        /// <param name="propertyName">The text to check.</param>
        /// <returns>True if the text can be used verbatim as a property name.</returns>
        public static bool IsValidPropertyName(string propertyName)
        {
            return propertyName.Length != 0 &&
                   !char.IsDigit(propertyName[0]) &&
                   propertyName.All(ch => char.IsLetter(ch) || char.IsDigit(ch) || ch == '_');
        }
    }
}