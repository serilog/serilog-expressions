namespace Serilog.Expressions.Ast
{
    class Spread : Member
    {
        public Expression Content { get; }

        public Spread(Expression content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return $"..{Content}";
        }
    }
}
