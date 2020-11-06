using System;

namespace Serilog.Templates.Ast
{
    class TemplateBlock : Template
    {
        public Template[] Elements { get; }

        public TemplateBlock(Template[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }
    }
}