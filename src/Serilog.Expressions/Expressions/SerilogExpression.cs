// Copyright Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="nameResolver">Optionally, a <see cref="NameResolver"/>
        /// with which to resolve function names that appear in the template.</param>
        /// <returns>A function that evaluates the expression in the context of a log event.</returns>
        public static CompiledExpression Compile(
            string expression,
            NameResolver? nameResolver = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (!TryCompileImpl(expression, nameResolver, out var filter, out var error))
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
        /// <remarks>Regular expression syntax errors currently generate exceptions instead of producing friendly
        /// errors.</remarks>
        public static bool TryCompile(
            string expression,
            [MaybeNullWhen(false)] out CompiledExpression result,
            [MaybeNullWhen(true)] out string error)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return TryCompileImpl(expression, null, out result, out error);
        }

        /// <summary>
        /// Create an evaluation function based on the provided expression.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <param name="result">A function that evaluates the expression in the context of a log event.</param>
        /// <param name="error">The reported error, if compilation was unsuccessful.</param>
        /// <param name="nameResolver">A <see cref="NameResolver"/>
        /// with which to resolve function names that appear in the template.</param>
        /// <returns>True if the function could be created; otherwise, false.</returns>
        /// <remarks>Regular expression syntax errors currently generate exceptions instead of producing friendly
        /// errors.</remarks>
        public static bool TryCompile(
            string expression,
            NameResolver nameResolver,
            [MaybeNullWhen(false)] out CompiledExpression result,
            [MaybeNullWhen(true)] out string error)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (nameResolver == null) throw new ArgumentNullException(nameof(nameResolver));
            return TryCompileImpl(expression, nameResolver, out result, out error);
        }
        
        static bool TryCompileImpl(
            string expression,
            NameResolver? nameResolver,
            [MaybeNullWhen(false)] out CompiledExpression result,
            [MaybeNullWhen(true)] out string error)
        {
            if (!ExpressionParser.TryParse(expression, out var root, out error))
            {
                result = null;
                return false;
            }

            result = ExpressionCompiler.Compile(root, DefaultFunctionNameResolver.Build(nameResolver));
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
        /// Determine if the specified text is a valid identifier.
        /// </summary>
        /// <param name="identifier">The text to check.</param>
        /// <returns>True if the text can be used verbatim as a property name.</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            return identifier.Length != 0 &&
                   !char.IsDigit(identifier[0]) &&
                   identifier.All(ch => char.IsLetter(ch) || char.IsDigit(ch) || ch == '_');
        }
    }
}
