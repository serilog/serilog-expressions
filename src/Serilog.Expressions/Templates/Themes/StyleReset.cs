using System;
using System.IO;

namespace Serilog.Templates.Themes
{
    readonly struct StyleReset : IDisposable
    {
        const string AnsiStyleResetSequence = "\x1b[0m";
        public const int ResetCharCount = 4;
        
        readonly TextWriter? _output;
        
        public StyleReset(TextWriter output)
        {
            _output = output;
        }

        public void Dispose()
        {
            _output?.Write(AnsiStyleResetSequence);
        }
    }
}
