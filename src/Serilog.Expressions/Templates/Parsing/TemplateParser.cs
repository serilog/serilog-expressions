using System;
using System.Diagnostics.CodeAnalysis;
using Serilog.Templates.Ast;

namespace Serilog.Templates.Parsing
{
    class TemplateParser
    {
        readonly TemplateTokenizer _tokenizer = new TemplateTokenizer();
        readonly TemplateTokenParsers _templateTokenParsers = new TemplateTokenParsers();
        
        public bool TryParse(
            string template,
            [MaybeNullWhen(false)] out Template parsed,
            [MaybeNullWhen(true)] out string error)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var tokenList = _tokenizer.TryTokenize(template);
            if (!tokenList.HasValue)
            {
                error = tokenList.ToString();
                parsed = null;
                return false;
            }
            
            var result = _templateTokenParsers.TryParse(tokenList.Value);
            if (!result.HasValue)
            {
                error = result.ToString();
                parsed = null;
                return false;
            }

            parsed = result.Value;
            error = null;
            return true;
        }
    }
}
