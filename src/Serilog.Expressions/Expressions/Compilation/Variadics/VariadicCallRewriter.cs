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

        protected override Expression Transform(CallExpression lx)
        {
            if (Operators.SameOperator(lx.OperatorName, Operators.OpSubstring) && lx.Operands.Length == 2)
            {
                var operands = lx.Operands
                    .Select(Transform)
                    .Concat(new[] {CallUndefined()})
                    .ToArray();
                return new CallExpression(lx.IgnoreCase, lx.OperatorName, operands);
            }

            if (Operators.SameOperator(lx.OperatorName, Operators.OpCoalesce))
            {
                if (lx.Operands.Length == 0)
                    return CallUndefined();
                if (lx.Operands.Length == 1)
                    return Transform(lx.Operands.Single());
                if (lx.Operands.Length > 2)
                {
                    var first = Transform(lx.Operands.First());
                    return new CallExpression(lx.IgnoreCase, lx.OperatorName, first,
                        Transform(new CallExpression(lx.IgnoreCase, lx.OperatorName, lx.Operands.Skip(1).ToArray())));
                }
            }

            if (Operators.SameOperator(lx.OperatorName, Operators.OpToString) &&
                lx.Operands.Length == 1)
            {
                return new CallExpression(lx.IgnoreCase, lx.OperatorName, lx.Operands[0], CallUndefined());
            }

            return base.Transform(lx);
        }

        static CallExpression CallUndefined()
        {
            return new CallExpression(false, Operators.OpUndefined);
        }
    }
}
