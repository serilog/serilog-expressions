using Serilog.Events;

namespace Serilog.Templates.Rendering;

/// <summary>
/// TODO
/// </summary>
public class DefaultPropertyValueRenderer : IPropertyValueRenderer
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="value"></param>
    /// <param name="output"></param>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public int Render(LogEventPropertyValue value, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        value.Render(output, format, formatProvider);
        return 0;
    }
}