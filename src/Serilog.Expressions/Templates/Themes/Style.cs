using System.IO;

namespace Serilog.Templates.Themes
{
    readonly struct Style
    {
        readonly string? _ansiStyle;

        public Style(string ansiStyle)
        {
            _ansiStyle = ansiStyle;
        }
        
        internal StyleReset Set(TextWriter output, ref int invisibleCharacterCount)
        {
            if (_ansiStyle != null)
            {
                output.Write(_ansiStyle);
                invisibleCharacterCount += _ansiStyle.Length;
                invisibleCharacterCount += StyleReset.ResetCharCount;

                return new StyleReset(output);
            }
            
            return default;
        }
    }
}
