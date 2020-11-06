namespace Serilog.Expressions.Ast
{
    class IndexerExpression : Expression
    {
        public Expression Receiver { get; }
        public Expression Index { get; }

        public IndexerExpression(Expression receiver, Expression index)
        {
            Receiver = receiver;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Receiver}[{Index}]";
        }
    }
}