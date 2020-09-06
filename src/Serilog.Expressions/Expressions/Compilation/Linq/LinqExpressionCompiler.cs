using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;
using Serilog.Expressions.Runtime;
using ConstantExpression = Serilog.Expressions.Ast.ConstantExpression;
using Expression = Serilog.Expressions.Ast.Expression;
using LambdaExpression = System.Linq.Expressions.LambdaExpression;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;
using LX = System.Linq.Expressions.Expression;

namespace Serilog.Expressions.Compilation.Linq
{
    class LinqExpressionCompiler : SerilogExpressionTransformer<Expression<CompiledExpression>>
    {
        static readonly LinqExpressionCompiler Instance = new LinqExpressionCompiler();
        
        static readonly IDictionary<string, MethodInfo> OperatorMethods = typeof(RuntimeOperators)
            .GetTypeInfo()
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);

        static readonly MethodInfo ConstructSequenceValueMethod = typeof(LinqExpressionCompiler)
            .GetMethod(nameof(ConstructSequenceValue), BindingFlags.Static | BindingFlags.Public);

        static readonly MethodInfo CoerceToScalarBooleanMethod = typeof(LinqExpressionCompiler)
            .GetMethod(nameof(CoerceToScalarBoolean), BindingFlags.Static | BindingFlags.Public);

        static readonly MethodInfo IndexOfMatchMethod = typeof(LinqExpressionCompiler)
            .GetMethod(nameof(IndexOfMatch), BindingFlags.Static | BindingFlags.Public);
        
        static readonly LogEventPropertyValue NegativeOne = new ScalarValue(-1);
        
        public static LogEventPropertyValue ConstructSequenceValue(LogEventPropertyValue[] elements)
        {
            // Avoid upsetting Serilog's (currently) fragile `SequenceValue.Render()`.
            if (elements.Any(el => el == null))
                return null;
            return new SequenceValue(elements);
        }
        
        public static LogEventPropertyValue CoerceToScalarBoolean(LogEventPropertyValue value)
        {
            if (value is ScalarValue sv && sv.Value is bool b)
                return RuntimeOperators.ScalarBoolean(b);
            return RuntimeOperators.ScalarBoolean(false);
        }
        
        public static LogEventPropertyValue IndexOfMatch(LogEventPropertyValue value, Regex regex)
        {
            if (value is ScalarValue scalar &&
                scalar.Value is string s)
            {
                var m = regex.Match(s);
                if (m.Success)
                    return new ScalarValue(m.Index);
                return NegativeOne;
            }

            return null;
        }
        
        public static CompiledExpression Compile(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return Instance.Transform(expression).Compile();
        }

        protected override Expression<CompiledExpression> Transform(CallExpression lx)
        {
            if (!OperatorMethods.TryGetValue(lx.OperatorName, out var m))
                throw new ArgumentException($"The function name `{lx.OperatorName}` was not recognised.");
            
            if (m.GetParameters().Length != lx.Operands.Length)
                throw new ArgumentException($"The function `{lx.OperatorName}` requires {m.GetParameters().Length} arguments.");

            var operands = lx.Operands.Select(Transform).ToArray();
            var context = LX.Parameter(typeof(LogEvent));

            var operandValues = operands.Select(o => Splice(o, context));
            var operandVars = new List<ParameterExpression>();
            var rtn = LX.Label(typeof(LogEventPropertyValue));

            var statements = new List<System.Linq.Expressions.Expression>();
            var first = true;
            var shortCircuit = false;
            System.Linq.Expressions.Expression shortCircuitElse = null;
            foreach (var op in operandValues)
            {
                if (shortCircuit)
                {
                    shortCircuitElse = LX.Call(CoerceToScalarBooleanMethod, op);
                    break;
                }
                    
                var opam = LX.Variable(typeof(LogEventPropertyValue));
                operandVars.Add(opam);
                statements.Add(LX.Assign(opam, op));

                if (first && Operators.SameOperator(lx.OperatorName, Operators.OpAnd))
                {
                    Expression<Func<LogEventPropertyValue, bool>> shortCircuitIf = v => !(v is ScalarValue) || !true.Equals(((ScalarValue)v).Value);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(LX.IfThen(scc, LX.Return(rtn, LX.Constant(new ScalarValue(false), typeof(LogEventPropertyValue)))));
                    shortCircuit = true;
                }

                if (first && Operators.SameOperator(lx.OperatorName, Operators.OpOr))
                {
                    Expression<Func<LogEventPropertyValue, bool>> shortCircuitIf = v => v is ScalarValue && true.Equals(((ScalarValue)v).Value);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(LX.IfThen(scc, LX.Return(rtn, LX.Constant(new ScalarValue(true), typeof(LogEventPropertyValue)))));
                    shortCircuit = true;
                }

                first = false;
            }

            statements.Add(LX.Return(rtn, shortCircuitElse ?? LX.Call(m, operandVars)));
            statements.Add(LX.Label(rtn, LX.Constant(null, typeof(LogEventPropertyValue))));

            return LX.Lambda<CompiledExpression>(
                LX.Block(typeof(LogEventPropertyValue), operandVars, statements),
                context);
        }

        protected override Expression<CompiledExpression> Transform(AccessorExpression spx)
        {
            var tgv = typeof(LinqExpressionCompiler).GetTypeInfo().GetMethod(nameof(TryGetStructurePropertyValue), BindingFlags.Static | BindingFlags.Public);
            var recv = Transform(spx.Receiver);

            var context = LX.Parameter(typeof(LogEvent));

            var r = LX.Variable(typeof(object));
            var str = LX.Variable(typeof(StructureValue));
            var result = LX.Variable(typeof(LogEventPropertyValue));

            var sx3 = LX.Call(tgv, str, LX.Constant(spx.MemberName, typeof(string)), result);

            var sx1 = LX.Condition(sx3,
                            result,
                            LX.Constant(null, typeof(LogEventPropertyValue)));

            var sx2 = LX.Block(typeof(LogEventPropertyValue),
                    LX.Assign(str, LX.TypeAs(r, typeof(StructureValue))),
                    LX.Condition(LX.Equal(str, LX.Constant(null, typeof(StructureValue))),
                        LX.Constant(null, typeof(LogEventPropertyValue)),
                        sx1));

            var assignR = LX.Assign(r, Splice(recv, context));
            var getValue = LX.Condition(LX.Equal(r, LX.Constant(null, typeof(LogEventPropertyValue))),
                LX.Constant(null, typeof(LogEventPropertyValue)),
                sx2);

            return LX.Lambda<CompiledExpression>(
                LX.Block(typeof(LogEventPropertyValue), new[] { r, str, result }, assignR, getValue),
                context);
        }

        static System.Linq.Expressions.Expression Splice(LambdaExpression lambda, params ParameterExpression[] newParameters)
        {
            var v = new ParameterReplacementVisitor(lambda.Parameters.ToArray(), newParameters);
            return v.Visit(lambda.Body);
        }

        protected override Expression<CompiledExpression> Transform(ConstantExpression cx)
        {
            return context => cx.Constant;
        }

        protected override Expression<CompiledExpression> Transform(AmbientPropertyExpression px)
        {
            if (px.IsBuiltIn)
            {
                if (px.PropertyName == BuiltInProperty.Level)
                    return context => new ScalarValue(context.Level);

                if (px.PropertyName == BuiltInProperty.Message)
                    return context => NormalizeBuiltInProperty(context.RenderMessage(null));

                if (px.PropertyName == BuiltInProperty.Exception)
                    return context => context.Exception == null ? null : new ScalarValue(context.Exception);

                if (px.PropertyName == BuiltInProperty.Timestamp)
                    return context => new ScalarValue(context.Timestamp);

                if (px.PropertyName == BuiltInProperty.MessageTemplate)
                    return context => NormalizeBuiltInProperty(context.MessageTemplate.Text);

                if (px.PropertyName == BuiltInProperty.Properties)
                    return context => new StructureValue(context.Properties.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)), null);

                // Also @Undefined
                return context => null;
            }

            var propertyName = px.PropertyName;

            return context => GetPropertyValue(context, propertyName);
        }

        static LogEventPropertyValue GetPropertyValue(LogEvent context, string propertyName)
        {
            if (!context.Properties.TryGetValue(propertyName, out var value))
                return null;

            return value;
        }

        public static bool TryGetStructurePropertyValue(StructureValue sv, string name, out LogEventPropertyValue value)
        {
            foreach (var prop in sv.Properties)
            {
                if (prop.Name == name)
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        static LogEventPropertyValue NormalizeBuiltInProperty(string rawValue)
        {
            // If a property like @Exception is null, it's not present at all, thus Undefined
            if (rawValue == null)
                return null;

            return new ScalarValue(rawValue);
        }

        protected override Expression<CompiledExpression> Transform(Ast.LambdaExpression lmx)
        {
            var context = LX.Parameter(typeof(LogEvent));
            var parms = lmx.Parameters.Select(px => Tuple.Create(px, LX.Parameter(typeof(LogEventPropertyValue), px.ParameterName))).ToList();
            var body = Splice(Transform(lmx.Body), context);
            var paramSwitcher = new ExpressionConstantMapper(parms.ToDictionary(px => (object)px.Item1, px => (System.Linq.Expressions.Expression)px.Item2));
            var rewritten = paramSwitcher.Visit(body);

            Type delegateType;
            if (lmx.Parameters.Length == 1)
                delegateType = typeof(Func<LogEventPropertyValue, LogEventPropertyValue>);
            else if (lmx.Parameters.Length == 2)
                delegateType = typeof(Func<LogEventPropertyValue, LogEventPropertyValue, LogEventPropertyValue>);
            else
                throw new NotSupportedException("Unsupported lambda signature.");

            var lambda = LX.Lambda(delegateType, rewritten!, parms.Select(px => px.Item2).ToArray());
            
            // Unfortunately, right now, functions need to be threaded through in constant scalar values :-D
            var constant = LX.New(typeof(ScalarValue).GetConstructor(new[] {typeof(object)})!,
                LX.Convert(lambda, typeof(object)));

            return LX.Lambda<CompiledExpression>(constant, context);
        }

        protected override Expression<CompiledExpression> Transform(Ast.ParameterExpression prx)
        {
            // Will be within a lambda, which will subsequently sub-in the actual value
            var context = LX.Parameter(typeof(LogEvent));
            var constant = LX.Constant(new ScalarValue(prx), typeof(LogEventPropertyValue));
            return LX.Lambda<CompiledExpression>(constant, context);
        }

        protected override Expression<CompiledExpression> Transform(IndexerWildcardExpression wx)
        {
            return context => null;
        }

        protected override Expression<CompiledExpression> Transform(ArrayExpression ax)
        {
            var context = LX.Parameter(typeof(LogEvent));
            var elements = ax.Elements.Select(Transform).Select(ex => Splice(ex, context)).ToArray();
            var arr = LX.NewArrayInit(typeof(LogEventPropertyValue), elements);
            var sv = LX.Call(ConstructSequenceValueMethod, arr);
            return LX.Lambda<CompiledExpression>(sv, context);
        }

        protected override Expression<CompiledExpression> Transform(IndexerExpression ix)
        {
            return Transform(new CallExpression(Operators.OpElementAt, ix.Receiver, ix.Index));
        }

        protected override Expression<CompiledExpression> Transform(IndexOfMatchExpression mx)
        {
            var context = LX.Parameter(typeof(LogEvent));
            var rx = LX.Constant(mx.Regex);
            var target = Splice(Transform(mx.Corpus), context);
            return LX.Lambda<CompiledExpression>(LX.Call(IndexOfMatchMethod, target, rx), context);
        }
    }
}
