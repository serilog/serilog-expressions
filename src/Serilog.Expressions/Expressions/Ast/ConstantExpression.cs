using System;
using System.Globalization;

namespace Serilog.Expressions.Ast
{ 
    class ConstantExpression : Expression
    {
        public ConstantExpression(object constantValue)
        {
            ConstantValue = constantValue;
        }

        public object ConstantValue { get; }
        
        public override string ToString()
        {
            switch (ConstantValue)
            {
                case string s:
                    return "'" + s.Replace("'", "''") + "'";
                case true:
                    return "true";
                case false:
                    return "false";
                case IFormattable formattable:
                    return formattable.ToString(null, CultureInfo.InvariantCulture);
                default:
                    return (ConstantValue ?? "null").ToString();
            }
        }
    }
}