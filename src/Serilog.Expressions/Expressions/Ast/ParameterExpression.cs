namespace Serilog.Expressions.Ast
{
    class ParameterExpression : Expression
    {
        public ParameterExpression(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }

        public override string ToString()
        {
            return "$$" + ParameterName;
        }
    }
}