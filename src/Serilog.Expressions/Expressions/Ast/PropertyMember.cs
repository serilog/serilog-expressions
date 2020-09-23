using Serilog.Events;

namespace Serilog.Expressions.Ast
{
    class PropertyMember : Member
    {
        public string Name { get; }
        public Expression Value { get; }

        public PropertyMember(string name, Expression value)
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