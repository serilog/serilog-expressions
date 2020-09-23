using Serilog.Events;

namespace Serilog.Expressions.Ast
{
    class Property : Member
    {
        public string Name { get; }
        public Expression Value { get; }

        public Property(string name, Expression value)
        {
            Name = name;
            Value = value;
        }
        
        public override string ToString()
        {
            return $"{new ScalarValue(Name)}: {Value}";
        }
    }
}