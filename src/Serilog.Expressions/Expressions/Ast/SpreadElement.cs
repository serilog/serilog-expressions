namespace Serilog.Expressions.Ast
{
    class SpreadElement : Element
    {
        public Expression Content { get; }

        public SpreadElement(Expression content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return $"..{Content}";
        }
    }
}