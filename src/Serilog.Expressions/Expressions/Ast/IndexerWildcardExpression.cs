using System;

namespace Serilog.Expressions.Ast
{
    class IndexerWildcardExpression : Expression
    {
        public IndexerWildcardExpression(IndexerWildcard wildcard)
        {
            Wildcard = wildcard;
        }

        public IndexerWildcard Wildcard { get; }

        public override string ToString()
        {
            switch (Wildcard)
            {
                case IndexerWildcard.Any:
                    return "?";
                case IndexerWildcard.All:
                    return "*";
                default:
                    throw new NotSupportedException("Unrecognized wildcard " + Wildcard);
            }
        }
    }
}