using System;
using Superpower;
using Superpower.Model;

namespace Serilog.Expressions.Parsing
{
    static class Combinators
    {
        public static TokenListParser<TKind, TResult> ChainModified<TKind, TResult, TOperator, TModifier>(
            TokenListParser<TKind, TOperator> @operator,
            TokenListParser<TKind, TResult> operand,
            TokenListParser<TKind, TModifier> modify,
            Func<TOperator, TResult, TResult, TModifier, TResult> apply)
        {
            if (@operator == null)
                throw new ArgumentNullException(nameof (@operator));
            if (operand == null)
                throw new ArgumentNullException(nameof (operand));
            if (modify == null) throw new ArgumentNullException(nameof(modify));
            if (apply == null)
                throw new ArgumentNullException(nameof (apply));
            
            return input =>
            {
                var parseResult = operand(input);
                if (!parseResult.HasValue )
                    return parseResult;

                var result = parseResult.Value;
                var remainder = parseResult.Remainder;

                var operatorResult = @operator(remainder);
                while (operatorResult.HasValue || operatorResult.SubTokenErrorPosition.HasValue || remainder != operatorResult.Remainder)
                {
                    // If operator read any input, but failed to read complete input, we return error
                    if (!operatorResult.HasValue) 
                        return TokenListParserResult.CastEmpty<TKind, TOperator, TResult>(operatorResult);

                    var operandResult = operand(operatorResult.Remainder);
                    remainder = operandResult.Remainder;

                    if (!operandResult.HasValue)
                        return operandResult;

                    var modifierResult = modify(remainder);
                    remainder = modifierResult.Remainder;

                    if (!modifierResult.HasValue)
                        return TokenListParserResult.CastEmpty<TKind, TModifier, TResult>(modifierResult);
                    
                    result = apply(operatorResult.Value, result, operandResult.Value, modifierResult.Value);

                    operatorResult = @operator(remainder);
                }

                return TokenListParserResult.Value(result, input, remainder);
            };
        }
    }
}
