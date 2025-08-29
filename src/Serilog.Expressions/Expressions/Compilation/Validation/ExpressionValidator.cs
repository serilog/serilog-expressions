// Copyright © Serilog Contributors
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

using System.Text;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Validation;

class ExpressionValidator : IdentityTransformer
{
    readonly NameResolver _nameResolver;
    readonly List<string> _errors = new();

    // Functions that support case-insensitive operations (have StringComparison parameter)
    static readonly HashSet<string> CaseInsensitiveCapableFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        // String operations
        Operators.OpContains,
        Operators.OpStartsWith,
        Operators.OpEndsWith,
        Operators.OpIndexOf,
        Operators.OpLastIndexOf,
        Operators.OpReplace,
        
        // Pattern matching
        Operators.OpIndexOfMatch,
        Operators.OpIsMatch,
        Operators.IntermediateOpLike,
        Operators.IntermediateOpNotLike,
        
        // Comparisons
        Operators.RuntimeOpEqual,
        Operators.RuntimeOpNotEqual,
        Operators.RuntimeOpIn,
        Operators.RuntimeOpNotIn,
        
        // Element access
        Operators.OpElementAt,
        
        // Special functions
        Operators.OpUndefined  // undefined() always accepts ci
    };

    ExpressionValidator(NameResolver nameResolver)
    {
        _nameResolver = nameResolver;
    }

    public static bool Validate(Expression expression, NameResolver nameResolver, out string? error)
    {
        var validator = new ExpressionValidator(nameResolver);
        validator.Transform(expression);

        if (validator._errors.Count == 0)
        {
            error = null;
            return true;
        }

        if (validator._errors.Count == 1)
        {
            error = validator._errors[0];
        }
        else
        {
            var sb = new StringBuilder("Multiple errors found: ");
            for (var i = 0; i < validator._errors.Count; i++)
            {
                if (i > 0)
                    sb.Append("; ");
                sb.Append(validator._errors[i]);
            }
            error = sb.ToString();
        }

        return false;
    }

    protected override Expression Transform(CallExpression call)
    {
        // Skip validation for intermediate operators (they get transformed later)
        if (!call.OperatorName.StartsWith("_Internal_"))
        {
            // Check for unknown function names
            if (!_nameResolver.TryResolveFunctionName(call.OperatorName, out _))
            {
                _errors.Add($"The function name `{call.OperatorName}` was not recognized.");
            }
        }

        // Check for invalid CI modifier usage
        if (call.IgnoreCase && !CaseInsensitiveCapableFunctions.Contains(call.OperatorName))
        {
            _errors.Add($"The function `{call.OperatorName}` does not support case-insensitive operation.");
        }

        // Validate regex patterns in IsMatch and IndexOfMatch
        if (Operators.SameOperator(call.OperatorName, Operators.OpIsMatch) ||
            Operators.SameOperator(call.OperatorName, Operators.OpIndexOfMatch))
        {
            ValidateRegexPattern(call);
        }

        return base.Transform(call);
    }

    void ValidateRegexPattern(CallExpression call)
    {
        if (call.Operands.Length != 2)
            return;

        var pattern = call.Operands[1];
        if (pattern is ConstantExpression { Constant: ScalarValue { Value: string s } })
        {
            try
            {
                var opts = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
                if (call.IgnoreCase)
                    opts |= RegexOptions.IgnoreCase;

                // Try to compile the regex with timeout to catch invalid patterns
                _ = new Regex(s, opts, TimeSpan.FromMilliseconds(100));
            }
            catch (ArgumentException ex)
            {
                _errors.Add($"Invalid regular expression in {call.OperatorName}: {ex.Message}");
            }
        }
    }
}