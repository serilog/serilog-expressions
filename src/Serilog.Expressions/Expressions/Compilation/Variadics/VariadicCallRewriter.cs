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

using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Variadics
{
    // Now a bit of a misnomer - handles variadic `coalesce()`, as well as optional arguments for other functions.
    class VariadicCallRewriter : IdentityTransformer
    {
        static readonly VariadicCallRewriter Instance = new VariadicCallRewriter();

        public static Expression Rewrite(Expression expression)
        {
            return Instance.Transform(expression);
        }

        protected override Expression Transform(CallExpression call)
        {
            if (Operators.SameOperator(call.OperatorName, Operators.OpSubstring) && call.Operands.Length == 2)
            {
                var operands = call.Operands
                    .Select(Transform)
                    .Concat(new[] {CallUndefined()})
                    .ToArray();
                return new CallExpression(call.IgnoreCase, call.OperatorName, operands);
            }

            if (Operators.SameOperator(call.OperatorName, Operators.OpCoalesce))
            {
                if (call.Operands.Length == 0)
                    return CallUndefined();
                if (call.Operands.Length == 1)
                    return Transform(call.Operands.Single());
                if (call.Operands.Length > 2)
                {
                    var first = Transform(call.Operands.First());
                    return new CallExpression(call.IgnoreCase, call.OperatorName, first,
                        Transform(new CallExpression(call.IgnoreCase, call.OperatorName, call.Operands.Skip(1).ToArray())));
                }
            }

            if (Operators.SameOperator(call.OperatorName, Operators.OpToString) &&
                call.Operands.Length == 1)
            {
                return new CallExpression(call.IgnoreCase, call.OperatorName, call.Operands[0], CallUndefined());
            }

            return base.Transform(call);
        }

        static CallExpression CallUndefined()
        {
            return new CallExpression(false, Operators.OpUndefined);
        }
    }
}
