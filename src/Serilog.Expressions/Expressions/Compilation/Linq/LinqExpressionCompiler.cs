using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Serilog.Events;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation.Transformations;
using Serilog.Expressions.Runtime;
using ConstantExpression = Serilog.Expressions.Ast.ConstantExpression;
using Expression = Serilog.Expressions.Ast.Expression;
using LambdaExpression = System.Linq.Expressions.LambdaExpression;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace Serilog.Expressions.Compilation.Linq
{
    class LinqExpressionCompiler : SerilogExpressionTransformer<Expression<CompiledExpression>>
    {
        static readonly IDictionary<string, MethodInfo> OperatorMethods = typeof(RuntimeOperators)
            .GetTypeInfo()
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);

        public static CompiledExpression Compile(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var compiler = new LinqExpressionCompiler();
            return compiler.Transform(expression).Compile();
        }

        protected override Expression<CompiledExpression> Transform(CallExpression lx)
        {
            if (!OperatorMethods.TryGetValue(lx.OperatorName, out var m))
                throw new ArgumentException($"The function name `{lx.OperatorName}` was not recognised; to search for text instead, enclose the filter in \"double quotes\".");

            if (m.GetParameters().Length != lx.Operands.Length)
                throw new ArgumentException($"The function `{lx.OperatorName}` requires {m.GetParameters().Length} arguments; to search for text instead, enclose the filter in \"double quotes\".");

            var acceptUndefined = m.GetCustomAttribute<AcceptUndefinedAttribute>() != null;
            var acceptNull = m.GetCustomAttribute<AcceptNullAttribute>() != null;
            var numericOnly = m.GetCustomAttribute<NumericAttribute>() != null;
            var numericComparable = m.GetCustomAttribute<NumericComparableAttribute>() != null;
            var booleanOnly = m.GetCustomAttribute<BooleanAttribute>() != null;
            var operands = lx.Operands.Select(Transform).ToArray();

            var returnUndefined = new List<Expression<Func<object, bool>>>();
            if (!acceptUndefined) returnUndefined.Add(v => v is Undefined);
            if (!acceptNull) returnUndefined.Add(v => v == null);
            if (numericOnly) returnUndefined.Add(v => !(v is decimal || v == null || v is Undefined));
            if (numericComparable) returnUndefined.Add(v => !(v is decimal || v == null || v is Undefined));
            if (booleanOnly) returnUndefined.Add(v => !(v is bool || v == null || v is Undefined));

            var context = System.Linq.Expressions.Expression.Parameter(typeof(LogEvent));

            var operandValues = operands.Select(o => Splice(o, context));
            var operandVars = new List<ParameterExpression>();
            var rtn = System.Linq.Expressions.Expression.Label(typeof(object));

            var statements = new List<System.Linq.Expressions.Expression>();
            var first = true;
            foreach (var op in operandValues)
            {
                var opam = System.Linq.Expressions.Expression.Variable(typeof(object));
                operandVars.Add(opam);
                statements.Add(System.Linq.Expressions.Expression.Assign(opam, op));

                if (first && Operators.SameOperator(lx.OperatorName, Operators.OpAnd))
                {
                    Expression<Func<object, bool>> shortCircuitIf = v => !true.Equals(v);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(System.Linq.Expressions.Expression.IfThen(scc, System.Linq.Expressions.Expression.Return(rtn, System.Linq.Expressions.Expression.Constant(false, typeof(object)))));
                }

                if (first && Operators.SameOperator(lx.OperatorName, Operators.OpOr))
                {
                    Expression<Func<object, bool>> shortCircuitIf = v => true.Equals(v);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(System.Linq.Expressions.Expression.IfThen(scc, System.Linq.Expressions.Expression.Return(rtn, System.Linq.Expressions.Expression.Constant(true, typeof(object)))));
                }

                var checks = returnUndefined.Select(fv => Splice(fv, opam)).ToArray();
                foreach (var check in checks)
                {
                    statements.Add(System.Linq.Expressions.Expression.IfThen(check, System.Linq.Expressions.Expression.Return(rtn, System.Linq.Expressions.Expression.Constant(Undefined.Value, typeof(object)))));
                }

                first = false;
            }

            statements.Add(System.Linq.Expressions.Expression.Return(rtn, System.Linq.Expressions.Expression.Call(m, operandVars)));
            statements.Add(System.Linq.Expressions.Expression.Label(rtn, System.Linq.Expressions.Expression.Constant(Undefined.Value, typeof(object))));

            return System.Linq.Expressions.Expression.Lambda<CompiledExpression>(
                System.Linq.Expressions.Expression.Block(typeof(object), operandVars, statements),
                context);
        }

        protected override Expression<CompiledExpression> Transform(AccessorExpression spx)
        {
            var tgv = typeof(LinqExpressionCompiler).GetTypeInfo().GetMethod(nameof(TryGetStructurePropertyValue), BindingFlags.Static | BindingFlags.Public);
            var norm = typeof(Representation).GetTypeInfo().GetMethod(nameof(Representation.Represent), BindingFlags.Static | BindingFlags.Public);

            var recv = Transform(spx.Receiver);

            var context = System.Linq.Expressions.Expression.Parameter(typeof(LogEvent));

            var r = System.Linq.Expressions.Expression.Variable(typeof(object));
            var str = System.Linq.Expressions.Expression.Variable(typeof(StructureValue));
            var result = System.Linq.Expressions.Expression.Variable(typeof(LogEventPropertyValue));

            var sx3 = System.Linq.Expressions.Expression.Call(tgv, str, System.Linq.Expressions.Expression.Constant(spx.MemberName, typeof(string)), result);

            var sx1 = System.Linq.Expressions.Expression.Condition(sx3,
                            System.Linq.Expressions.Expression.Call(norm, result),
                            System.Linq.Expressions.Expression.Constant(Undefined.Value, typeof(object)));

            var sx2 = System.Linq.Expressions.Expression.Block(typeof(object),
                    System.Linq.Expressions.Expression.Assign(str, System.Linq.Expressions.Expression.TypeAs(r, typeof(StructureValue))),
                    System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Equal(str, System.Linq.Expressions.Expression.Constant(null, typeof(StructureValue))),
                        System.Linq.Expressions.Expression.Constant(Undefined.Value, typeof(object)),
                        sx1));

            var assignR = System.Linq.Expressions.Expression.Assign(r, Splice(recv, context));
            var getValue = System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.TypeIs(r, typeof(Undefined)),
                System.Linq.Expressions.Expression.Constant(Undefined.Value, typeof(object)),
                sx2);

            return System.Linq.Expressions.Expression.Lambda<CompiledExpression>(
                System.Linq.Expressions.Expression.Block(typeof(object), new[] { r, str, result }, assignR, getValue),
                context);

            //return context =>
            //{
            //    var r = recv(context);
            //    if (r is Undefined)
            //        return Undefined.Value;

            //    var str = r as StructureValue;
            //    if (str == null)
            //        return Undefined.Value;

            //    LogEventPropertyValue result;
            //    if (!str.Properties.TryGetValue(spx.PropertyName, out result))
            //        return Undefined.Value;

            //    return Represent(result);
            //};
        }

        static System.Linq.Expressions.Expression Splice(LambdaExpression lambda, params ParameterExpression[] newParameters)
        {
            var v = new ParameterReplacementVisitor(lambda.Parameters.ToArray(), newParameters);
            return v.Visit(lambda.Body);
        }

        protected override Expression<CompiledExpression> Transform(ConstantExpression cx)
        {
            return context => cx.ConstantValue;
        }

        protected override Expression<CompiledExpression> Transform(AmbientPropertyExpression px)
        {
            if (px.IsBuiltIn)
            {
                if (px.PropertyName == BuiltInProperty.Level)
                    return context => context.Level.ToString();

                if (px.PropertyName == BuiltInProperty.Message)
                    return context => NormalizeBaseDocumentProperty(context.RenderMessage(null));

                if (px.PropertyName == BuiltInProperty.Exception)
                    return context => NormalizeBaseDocumentProperty(context.Exception == null ? null : context.Exception.ToString());

                if (px.PropertyName == BuiltInProperty.Timestamp)
                    return context => context.Timestamp.ToString("o");

                if (px.PropertyName == BuiltInProperty.MessageTemplate)
                    return context => NormalizeBaseDocumentProperty(context.MessageTemplate.Text);

                if (px.PropertyName == BuiltInProperty.Properties)
                    return context => new StructureValue(context.Properties.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)), null);

                return context => Undefined.Value;
            }

            var propertyName = px.PropertyName;

            return context => GetPropertyValue(context, propertyName);
        }

        static object GetPropertyValue(LogEvent context, string propertyName)
        {
            if (!context.Properties.TryGetValue(propertyName, out var value))
                return Undefined.Value;

            return Representation.Represent(value);
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

        static object NormalizeBaseDocumentProperty(string rawValue)
        {
            // If a property like @Exception is null, it's not present at all, thus Undefined
            if (rawValue == null)
                return Undefined.Value;

            return rawValue;
        }

        protected override Expression<CompiledExpression> Transform(Ast.LambdaExpression lmx)
        {
            var context = System.Linq.Expressions.Expression.Parameter(typeof(LogEvent));
            var parms = lmx.Parameters.Select(px => Tuple.Create(px, System.Linq.Expressions.Expression.Parameter(typeof(object), px.ParameterName))).ToList();
            var body = Splice(Transform(lmx.Body), context);
            var paramSwitcher = new ExpressionConstantMapper(parms.ToDictionary(px => (object)px.Item1, px => (System.Linq.Expressions.Expression)px.Item2));
            var rewritten = paramSwitcher.Visit(body);

            Type delegateType;
            if (lmx.Parameters.Length == 1)
                delegateType = typeof(Func<object, object>);
            else if (lmx.Parameters.Length == 2)
                delegateType = typeof(Func<object, object, object>);
            else
                throw new NotSupportedException("Unsupported lambda signature");

            var lambda = System.Linq.Expressions.Expression.Lambda(delegateType, rewritten, parms.Select(px => px.Item2).ToArray());

            return System.Linq.Expressions.Expression.Lambda<CompiledExpression>(lambda, context);
        }

        protected override Expression<CompiledExpression> Transform(Ast.ParameterExpression prx)
        {
            // Will be within a lambda, which will subsequently sub-in the actual value
            var context = System.Linq.Expressions.Expression.Parameter(typeof(LogEvent));
            var constant = System.Linq.Expressions.Expression.Constant(prx, typeof(object));
            return System.Linq.Expressions.Expression.Lambda<CompiledExpression>(constant, context);
        }

        protected override Expression<CompiledExpression> Transform(IndexerWildcardExpression wx)
        {
            return context => Undefined.Value;
        }

        protected override Expression<CompiledExpression> Transform(ArrayExpression ax)
        {
            var context = System.Linq.Expressions.Expression.Parameter(typeof(LogEvent));
            var elements = ax.Elements.Select(Transform).Select(ex => Splice(ex, context)).ToArray();
            var arr = System.Linq.Expressions.Expression.NewArrayInit(typeof(object), elements);
            var sv = System.Linq.Expressions.Expression.Call(OperatorMethods[Operators.RuntimeOpNewSequence], arr);
            return System.Linq.Expressions.Expression.Lambda<CompiledExpression>(System.Linq.Expressions.Expression.Convert(sv, typeof(object)), context);
        }

        protected override Expression<CompiledExpression> Transform(IndexerExpression ix)
        {
            return Transform(new CallExpression(Operators.OpElementAt, ix.Receiver, ix.Index));
        }
    }
}
