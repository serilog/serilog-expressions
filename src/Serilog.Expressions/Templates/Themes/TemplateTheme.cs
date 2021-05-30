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
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Templates.Themes
{
    /// <summary>
    /// A template theme using the ANSI terminal escape sequences.
    /// </summary>
    public class TemplateTheme
    {
        /// <summary>
        /// A 256-color theme along the lines of Visual Studio Code.
        /// </summary>
        public static TemplateTheme Code { get; } = TemplateThemes.Code;

        /// <summary>
        /// A theme using only gray, black and white.
        /// </summary>
        public static TemplateTheme Grayscale { get; } = TemplateThemes.Grayscale;

        /// <summary>
        /// A theme in the style of the original <i>Serilog.Sinks.Literate</i>.
        /// </summary>
        public static TemplateTheme Literate { get; } = TemplateThemes.Literate;

        internal static TemplateTheme None { get; } = new TemplateTheme(new Dictionary<TemplateThemeStyle, string>());

        readonly IReadOnlyDictionary<TemplateThemeStyle, Style> _styles;

        /// <summary>
        /// Construct a theme given a set of styles.
        /// </summary>
        /// <param name="styles">Styles to apply within the theme.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="styles"/> is <code>null</code></exception>
        public TemplateTheme(IReadOnlyDictionary<TemplateThemeStyle, string> styles)
        {
            if (styles is null) throw new ArgumentNullException(nameof(styles));
            _styles = styles.ToDictionary(kv => kv.Key, kv => new Style(kv.Value));
        }

        internal Style GetStyle(TemplateThemeStyle templateThemeStyle)
        {
            _styles.TryGetValue(templateThemeStyle, out var style);
            return style;
        }
    }
}
