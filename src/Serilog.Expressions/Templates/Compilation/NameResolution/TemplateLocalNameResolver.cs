using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Templates.Ast;

// ReSharper disable MemberCanBeMadeStatic.Local, SuggestBaseTypeForParameter

namespace Serilog.Templates.Compilation.NameResolution
{
    class TemplateLocalNameBinder
    {
        public static Template BindLocalValueNames(Template template)
        {
            var binder = new TemplateLocalNameBinder();
            return binder.Transform(template, new Stack<string>());
        }

        Template Transform(Template template, Stack<string> locals)
        {
            return template switch
            {
                TemplateBlock block => Transform(block, locals),
                LiteralText text => text,
                FormattedExpression fx => Transform(fx, locals),
                Conditional cond => Transform(cond, locals),
                Repetition rep => Transform(rep, locals),
                _ => throw new NotSupportedException("Unsupported template type.")
            };
        }

        Template Transform(TemplateBlock block, Stack<string> locals)
        {
            return new TemplateBlock(block.Elements
                .Select(e => Transform(e, locals))
                .ToArray());
        }

        Template Transform(FormattedExpression fx, Stack<string> locals)
        {
            if (locals.Count == 0)
                return fx;
            
            return new FormattedExpression(
                ExpressionLocalNameBinder.BindLocalValueNames(fx.Expression, locals),
                fx.Format,
                fx.Alignment);
        }

        Template Transform(Conditional cond, Stack<string> locals)
        {
            return new Conditional(
                cond.Condition,
                cond.Consequent,
                cond.Alternative != null ? Transform(cond.Alternative, locals) : null);
        }

        Template Transform(Repetition rep, Stack<string> locals)
        {
            var orig = locals.Count;
            if (rep.KeyOrElementName != null)
                locals.Push(rep.KeyOrElementName);
            if (rep.ValueName != null)
                locals.Push(rep.ValueName);

            var body = Transform(rep.Body, locals);

            while (locals.Count != orig)
                locals.Pop();
            
            return new Repetition(
                rep.Enumerable,
                rep.KeyOrElementName,
                rep.ValueName,
                body,
                rep.Separator != null ? Transform(rep.Separator, locals) : null,
                rep.Alternative != null ? Transform(rep.Alternative, locals) : null);
        }
    }
}
