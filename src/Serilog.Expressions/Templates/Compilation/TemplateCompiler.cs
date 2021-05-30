// Copyright © Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using Serilog.Expressions;
using Serilog.Expressions.Ast;
using Serilog.Expressions.Compilation;
using Serilog.Templates.Ast;
using Serilog.Templates.Themes;

namespace Serilog.Templates.Compilation
{
    static class TemplateCompiler
    {
        public static CompiledTemplate Compile(Template template, IFormatProvider? formatProvider, NameResolver nameResolver, TemplateTheme theme)
        {
            return template switch
            {
                LiteralText text => new CompiledLiteralText(text.Text, theme),
                FormattedExpression { Expression: AmbientNameExpression { IsBuiltIn: true, PropertyName: BuiltInProperty.Level} } level => new CompiledLevelToken(
                    level.Format, level.Alignment, theme),
                FormattedExpression
                {
                    Expression: AmbientNameExpression { IsBuiltIn: true, PropertyName: BuiltInProperty.Exception }, 
                    Alignment: null,
                    Format: null
                } => new CompiledExceptionToken(theme),
                FormattedExpression
                {
                    Expression: AmbientNameExpression { IsBuiltIn: true, PropertyName: BuiltInProperty.Message }, 
                    Format: null
                } message => new CompiledMessageToken(formatProvider, message.Alignment, theme),
                FormattedExpression expression => new CompiledFormattedExpression(
                    ExpressionCompiler.Compile(expression.Expression, nameResolver), expression.Format, expression.Alignment, formatProvider, theme),
                TemplateBlock block => new CompiledTemplateBlock(block.Elements.Select(e => Compile(e, formatProvider, nameResolver, theme)).ToArray()),
                Conditional conditional => new CompiledConditional(
                    ExpressionCompiler.Compile(conditional.Condition, nameResolver),
                    Compile(conditional.Consequent, formatProvider, nameResolver, theme),
                    conditional.Alternative == null ? null : Compile(conditional.Alternative, formatProvider, nameResolver, theme)),
                Repetition repetition => new CompiledRepetition(
                    ExpressionCompiler.Compile(repetition.Enumerable, nameResolver),
                    repetition.BindingNames.Length > 0 ? repetition.BindingNames[0] : null,
                    repetition.BindingNames.Length > 1 ? repetition.BindingNames[1] : null,
                    Compile(repetition.Body, formatProvider, nameResolver, theme),
                    repetition.Delimiter == null ? null : Compile(repetition.Delimiter, formatProvider, nameResolver, theme),
                    repetition.Alternative == null ? null : Compile(repetition.Alternative, formatProvider, nameResolver, theme)),
                _ => throw new NotSupportedException()
            };
        }
    }
}