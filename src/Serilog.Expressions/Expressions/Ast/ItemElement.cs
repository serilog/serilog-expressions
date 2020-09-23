namespace Serilog.Expressions.Ast
{
    class ItemElement : Element
    {
        public Expression Value { get; }

        public ItemElement(Expression value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}