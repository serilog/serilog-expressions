using Serilog.Expressions.Ast;

namespace Serilog.Expressions.Compilation.Costing
{
    class EvaluationCosting
    {
        public Expression Expression { get; }
        public double Costing { get; }
        public string ReorderableOperator { get; }
        public EvaluationCosting[] ReorderableOperands { get; }

        public EvaluationCosting(Expression expression, double costing, string reorderableOperator = null, EvaluationCosting[] reorderableOperands = null)
        {
            Expression = expression;
            Costing = costing;
            ReorderableOperator = reorderableOperator;
            ReorderableOperands = reorderableOperands;
        }
    }
}
