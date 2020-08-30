using System;

namespace Serilog.Expressions.Ast
{
    class AccessorExpression : Expression
    {
        public AccessorExpression(Expression receiver, string memberName)
        {
            MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            Receiver = receiver;
        }

        public string MemberName { get; }

        public Expression Receiver { get; }

        public override string ToString()
        {
            if (SerilogExpression.IsValidPropertyName(MemberName))
                return Receiver + "." + MemberName;

            return $"{Receiver}['{SerilogExpression.EscapeStringContent(MemberName)}']";
        }
    }
}
