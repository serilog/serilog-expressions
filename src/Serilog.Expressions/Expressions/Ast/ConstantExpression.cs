using System;
using System.Globalization;
using Serilog.Events;

namespace Serilog.Expressions.Ast
{ 
    class ConstantExpression : Expression
    {
        public ConstantExpression(LogEventPropertyValue constant)
        {
            Constant = constant ?? throw new ArgumentNullException(nameof(constant));
        }

        public LogEventPropertyValue Constant { get; }
        
        public override string ToString()
        {
            if (Constant is ScalarValue sv)
            {
                switch (sv.Value)
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
                        return (sv.Value ?? "null").ToString();
                }
            }

            return Constant.ToString();
        }
    }
}