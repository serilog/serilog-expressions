using Serilog.Parsing;

namespace Serilog.Templates.Rendering
{
    static class AlignmentExtensions
    {
        public static Alignment Widen(this Alignment alignment, int amount)
        {
            return new(alignment.Direction, alignment.Width + amount);
        }
    }
}
