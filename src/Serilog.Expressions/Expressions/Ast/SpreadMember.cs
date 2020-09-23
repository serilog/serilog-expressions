namespace Serilog.Expressions.Ast
{
    class SpreadMember : Member
    {
        public Expression Content { get; }

        public SpreadMember(Expression content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return $"..{Content}";
        }
    }
}
