using System.Collections.Generic;
using System.Linq;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;

namespace Serilog.Expressions.Compilation.Costing
{
    class EvaluationCostReordering : SerilogExpressionTransformer<EvaluationCosting>
    {
        static readonly string[] PotentialTextComparisons =
        {
            Operators.OpStartsWith, Operators.OpEndsWith, Operators.OpContains, Operators.OpIndexOf,
            Operators.OpEqual, Operators.OpNotEqual
        };
        
        public static Expression Reorder(Expression expression)
        {
            var costing = new EvaluationCostReordering();
            return costing.Transform(expression).Expression;
        }

        protected override EvaluationCosting Transform(AmbientPropertyExpression px)
        {
            if (px.PropertyName == BuiltInProperty.Exception && px.IsBuiltIn)
                return new EvaluationCosting(px, 100);

            if (px.PropertyName == BuiltInProperty.Level && px.IsBuiltIn)
                return new EvaluationCosting(px, 5);

            if (px.IsBuiltIn)
                return new EvaluationCosting(px, 1);

            return new EvaluationCosting(px, 10);
        }

        protected override EvaluationCosting Transform(ParameterExpression prx)
        {
            return new EvaluationCosting(prx, 0.1);
        }

        protected override EvaluationCosting Transform(IndexerWildcardExpression wx)
        {
            return new EvaluationCosting(wx, 0);
        }

        protected override EvaluationCosting Transform(LambdaExpression lmx)
        {
            var body = Transform(lmx.Body);
            return new EvaluationCosting(
                new LambdaExpression(lmx.Parameters, body.Expression),
                body.Costing + 0.1);
        }

        protected override EvaluationCosting Transform(AccessorExpression spx)
        {
            var receiver = Transform(spx.Receiver);
            return new EvaluationCosting(
                new AccessorExpression(receiver.Expression, spx.MemberName),
                receiver.Costing + 0.1);
        }

        protected override EvaluationCosting Transform(ConstantExpression cx)
        {
            return new EvaluationCosting(cx, 0.001);
        }

        protected override EvaluationCosting Transform(CallExpression lx)
        {
            var operands = lx.Operands.Select(Transform).ToArray();
            var operatorName = lx.OperatorName;

            if ((Operators.SameOperator(operatorName, Operators.OpAnd) || Operators.SameOperator(operatorName, Operators.OpOr)) && operands.Length == 2)
            {
                var reorderable = new List<EvaluationCosting>();
                foreach (var operand in operands)
                {
                    if (operand.ReorderableOperator?.ToLowerInvariant() == operatorName)
                    {
                        foreach (var ro in operand.ReorderableOperands)
                            reorderable.Add(ro);
                    }
                    else
                    {
                        reorderable.Add(operand);
                    }
                }

                var remaining = new Stack<EvaluationCosting>(reorderable.OrderBy(r => r.Costing));
                var top = remaining.Pop();
                var rhsExpr = top.Expression;
                var rhsCosting = top.Costing;

                while (remaining.Count != 0)
                {
                    var lhs = remaining.Pop();
                    rhsExpr = new CallExpression(lx.OperatorName, lhs.Expression, rhsExpr);
                    rhsCosting = lhs.Costing + 0.75 * rhsCosting;
                }

                return new EvaluationCosting(
                    rhsExpr,
                    rhsCosting,
                    lx.OperatorName,
                    reorderable.ToArray());
            }

            if ((operatorName == Operators.RuntimeOpAny || operatorName == Operators.RuntimeOpAll) && operands.Length == 2)
            {
                return new EvaluationCosting(
                    new CallExpression(lx.OperatorName, operands[0].Expression, operands[1].Expression),
                    operands[0].Costing + 0.1 + operands[1].Costing * 7);
            }

            if (PotentialTextComparisons.Any(op => Operators.SameOperator(operatorName, op)) && operands.Length == 2)
            {
                return new EvaluationCosting(
                    new CallExpression(lx.OperatorName, operands[0].Expression, operands[1].Expression),
                    operands[0].Costing + operands[1].Costing + 10);
            }

            return new EvaluationCosting(
                new CallExpression(lx.OperatorName, operands.Select(o => o.Expression).ToArray()),
                operands.Sum(o => o.Costing) + 0.1);
        }

        protected override EvaluationCosting Transform(ArrayExpression ax)
        {
            return new EvaluationCosting(ax, 0.2);
        }

        protected override EvaluationCosting Transform(IndexerExpression ix)
        {
            return new EvaluationCosting(ix, Transform(ix.Receiver).Costing + Transform(ix.Index).Costing + 0.1);
        }
    }
}
