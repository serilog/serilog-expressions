﻿using System;
using System.Globalization;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace Serilog.Expressions.Parsing
{
    static class ExpressionTokenParsers
    {
        public static TokenListParserResult<ExpressionToken, Expression> TryParse(
            TokenList<ExpressionToken> input)
        {
            return Expr.AtEnd().TryParse(input);
        }

        static readonly TokenListParser<ExpressionToken, string> Add = Token.EqualTo(ExpressionToken.Plus).Value(Operators.OpAdd);
        static readonly TokenListParser<ExpressionToken, string> Subtract = Token.EqualTo(ExpressionToken.Minus).Value(Operators.OpSubtract);
        static readonly TokenListParser<ExpressionToken, string> Multiply = Token.EqualTo(ExpressionToken.Asterisk).Value(Operators.OpMultiply);
        static readonly TokenListParser<ExpressionToken, string> Divide = Token.EqualTo(ExpressionToken.ForwardSlash).Value(Operators.OpDivide);
        static readonly TokenListParser<ExpressionToken, string> Modulo = Token.EqualTo(ExpressionToken.Percent).Value(Operators.OpModulo);
        static readonly TokenListParser<ExpressionToken, string> Power = Token.EqualTo(ExpressionToken.Caret).Value(Operators.OpPower);
        static readonly TokenListParser<ExpressionToken, string> And = Token.EqualTo(ExpressionToken.And).Value(Operators.OpAnd);
        static readonly TokenListParser<ExpressionToken, string> Or = Token.EqualTo(ExpressionToken.Or).Value(Operators.OpOr);
        static readonly TokenListParser<ExpressionToken, string> Lte = Token.EqualTo(ExpressionToken.LessThanOrEqual).Value(Operators.OpLessThanOrEqual);
        static readonly TokenListParser<ExpressionToken, string> Lt = Token.EqualTo(ExpressionToken.LessThan).Value(Operators.OpLessThan);
        static readonly TokenListParser<ExpressionToken, string> Gt = Token.EqualTo(ExpressionToken.GreaterThan).Value(Operators.OpGreaterThan);
        static readonly TokenListParser<ExpressionToken, string> Gte = Token.EqualTo(ExpressionToken.GreaterThanOrEqual).Value(Operators.OpGreaterThanOrEqual);
        static readonly TokenListParser<ExpressionToken, string> Eq = Token.EqualTo(ExpressionToken.Equal).Value(Operators.OpEqual);
        static readonly TokenListParser<ExpressionToken, string> Neq = Token.EqualTo(ExpressionToken.NotEqual).Value(Operators.OpNotEqual);
        static readonly TokenListParser<ExpressionToken, string> Negate = Token.EqualTo(ExpressionToken.Minus).Value(Operators.OpNegate);
        static readonly TokenListParser<ExpressionToken, string> Not = Token.EqualTo(ExpressionToken.Not).Value(Operators.OpNot);
        static readonly TokenListParser<ExpressionToken, string> Like = Token.EqualTo(ExpressionToken.Like).Value(Operators.IntermediateOpSqlLike);
        static readonly TokenListParser<ExpressionToken, string> NotLike = Not.IgnoreThen(Like).Value(Operators.IntermediateOpSqlNotLike);
        static readonly TokenListParser<ExpressionToken, string> In = Token.EqualTo(ExpressionToken.In).Value(Operators.RuntimeOpSqlIn);
        static readonly TokenListParser<ExpressionToken, string> NotIn = Not.IgnoreThen(In).Value(Operators.IntermediateOpSqlNotIn);
        static readonly TokenListParser<ExpressionToken, string> Is = Token.EqualTo(ExpressionToken.Is).Value(Operators.IntermediateOpSqlIs);

        static readonly TokenListParser<ExpressionToken, Func<Expression, Expression>> PropertyPathStep =
            Token.EqualTo(ExpressionToken.Period)
                .IgnoreThen(Token.EqualTo(ExpressionToken.Identifier))
                .Then(n => Parse.Return<ExpressionToken, Func<Expression, Expression>>(r => new AccessorExpression(r, n.ToStringValue())));

        static readonly TokenListParser<ExpressionToken, Expression> Wildcard =
            Token.EqualTo(ExpressionToken.QuestionMark).Value((Expression)new IndexerWildcardExpression(IndexerWildcard.Any))
                .Or(Token.EqualTo(ExpressionToken.Asterisk).Value((Expression)new IndexerWildcardExpression(IndexerWildcard.All)));

        static readonly TokenListParser<ExpressionToken, Func<Expression, Expression>> PropertyPathIndexerStep =
            from open in Token.EqualTo(ExpressionToken.LBracket)
            from indexer in Wildcard.Or(Parse.Ref(() => Expr))
            from close in Token.EqualTo(ExpressionToken.RBracket)
            select new Func<Expression, Expression>(r => new IndexerExpression(r, indexer));

        static readonly TokenListParser<ExpressionToken, Expression> Function =
            (from name in Token.EqualTo(ExpressionToken.Identifier)
            from lparen in Token.EqualTo(ExpressionToken.LParen)
                from expr in Parse.Ref(() => Expr).ManyDelimitedBy(Token.EqualTo(ExpressionToken.Comma))
             from rparen in Token.EqualTo(ExpressionToken.RParen)
            select (Expression)new CallExpression(name.ToStringValue(), expr)).Named("function");

        static readonly TokenListParser<ExpressionToken, Expression> ArrayLiteral =
        (from lbracket in Token.EqualTo(ExpressionToken.LBracket)
            from expr in Parse.Ref(() => Expr).ManyDelimitedBy(Token.EqualTo(ExpressionToken.Comma))
            from rbracket in Token.EqualTo(ExpressionToken.RBracket)
            select (Expression)new ArrayExpression(expr)).Named("array");

        static readonly TokenListParser<ExpressionToken, Expression> RootProperty =
            (from notFunction in Parse.Not(Token.EqualTo(ExpressionToken.Identifier).IgnoreThen(Token.EqualTo(ExpressionToken.LParen)))
                from p in Token.EqualTo(ExpressionToken.BuiltInIdentifier).Select(b => (Expression) new AmbientPropertyExpression(b.ToStringValue().Substring(1), true))
                    .Or(Token.EqualTo(ExpressionToken.Identifier).Select(t => (Expression) new AmbientPropertyExpression(t.ToStringValue(), false)))
                select p).Named("property");

        static readonly TokenListParser<ExpressionToken, Expression> String =
            Token.EqualTo(ExpressionToken.String)
                .Apply(ExpressionTextParsers.String)
                .Select(s => (Expression)new ConstantExpression(new ScalarValue(s)));

        static readonly TokenListParser<ExpressionToken, Expression> HexNumber =
            Token.EqualTo(ExpressionToken.HexNumber)
                .Apply(ExpressionTextParsers.HexInteger)
                .SelectCatch(n => ulong.Parse(n, NumberStyles.HexNumber, CultureInfo.InvariantCulture), "the numeric literal is too large")
                .Select(u => (Expression)new ConstantExpression(new ScalarValue((decimal)u)));

        static readonly TokenListParser<ExpressionToken, Expression> Number =
            Token.EqualTo(ExpressionToken.Number)
                .Apply(ExpressionTextParsers.Real)
                .SelectCatch(n => decimal.Parse(n.ToStringValue(), CultureInfo.InvariantCulture), "the numeric literal is too large")
                .Select(d => (Expression)new ConstantExpression(new ScalarValue(d)));

        static readonly TokenListParser<ExpressionToken, Expression> Literal =
            String
                .Or(Number)
                .Or(HexNumber)
                .Or(Token.EqualTo(ExpressionToken.True).Value((Expression)new ConstantExpression(new ScalarValue(true))))
                .Or(Token.EqualTo(ExpressionToken.False).Value((Expression)new ConstantExpression(new ScalarValue(false))))
                .Or(Token.EqualTo(ExpressionToken.Null).Value((Expression)new ConstantExpression(new ScalarValue(null))))
                .Named("literal");

        static readonly TokenListParser<ExpressionToken, Expression> Item = Literal.Or(RootProperty).Or(Function).Or(ArrayLiteral);

        static readonly TokenListParser<ExpressionToken, Expression> Factor =
            (from lparen in Token.EqualTo(ExpressionToken.LParen)
             from expr in Parse.Ref(() => Expr)
             from rparen in Token.EqualTo(ExpressionToken.RParen)
             select expr)
                .Or(Item);

        static readonly TokenListParser<ExpressionToken, Expression> Path =
            from root in Factor
            from path in PropertyPathStep.Or(PropertyPathIndexerStep).Many()
            select path.Aggregate(root, (o, f) => f(o));

        static readonly TokenListParser<ExpressionToken, Expression> Operand =
            (from op in Negate.Or(Not)
                from path in Path
                select MakeUnary(op, path)).Or(Path).Named("expression");

        static readonly TokenListParser<ExpressionToken, Expression> InnerTerm = Parse.Chain(Power, Operand, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Term = Parse.Chain(Multiply.Or(Divide).Or(Modulo), InnerTerm, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Comparand = Parse.Chain(Add.Or(Subtract), Term, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Comparison = Parse.Chain(Is.Or(NotLike.Try().Or(Like)).Or(NotIn.Try().Or(In)).Or(Lte.Or(Neq).Or(Lt)).Or(Gte.Or(Gt)).Or(Eq), Comparand, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Conjunction = Parse.Chain(And, Comparison, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Disjunction = Parse.Chain(Or, Conjunction, MakeBinary);

        static readonly TokenListParser<ExpressionToken, Expression> Expr = Disjunction;

        static Expression MakeBinary(string operatorName, Expression leftOperand, Expression rightOperand)
        {
            return new CallExpression(operatorName, leftOperand, rightOperand);
        }

        static Expression MakeUnary(string operatorName, Expression operand)
        {
            return new CallExpression(operatorName, operand);
        }
    }
}
