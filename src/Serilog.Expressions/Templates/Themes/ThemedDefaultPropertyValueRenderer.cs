using System.Globalization;
using Serilog.Data;
using Serilog.Events;
using Serilog.Templates.Rendering;

// ReSharper disable ForCanBeConvertedToForeach

namespace Serilog.Templates.Themes;

/// <summary>
/// TODO
/// </summary>
public class ThemedDefaultPropertyValueRenderer : LogEventPropertyValueVisitor<ThemedDefaultPropertyValueRenderer.ValueFormatterVisitorState, int>, IPropertyValueRenderer
{
    readonly Style _null, _bool, _num, _string, _scalar, _tertiary, _name;

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="theme"></param>
    public ThemedDefaultPropertyValueRenderer(TemplateTheme theme)
    {
        _null = theme.GetStyle(TemplateThemeStyle.Null);
        _bool = theme.GetStyle(TemplateThemeStyle.Boolean);
        _num = theme.GetStyle(TemplateThemeStyle.Number);
        _string = theme.GetStyle(TemplateThemeStyle.String);
        _scalar = theme.GetStyle(TemplateThemeStyle.Scalar);
        _tertiary = theme.GetStyle(TemplateThemeStyle.TertiaryText);
        _name = theme.GetStyle(TemplateThemeStyle.Name);
    }

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
        return Visit(new ValueFormatterVisitorState(output, format, formatProvider), value);
    }

    int RenderScalarValue(ScalarValue scalar, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        var value = scalar.Value;
        var count = 0;

        var scalarStyle = value switch
        {
            null => _null,
            string => _string,
            int or uint or long or ulong or decimal or byte or sbyte or short or ushort or float or double => _num,
            bool => _bool,
            _ => _scalar
        };

        using (scalarStyle.Set(output, ref count))
        {
            if (value == null)
            {
                output.Write("null");
                return count;
            }

            if (value is string s)
            {
                if (format != "l")
                {
                    output.Write('"');
                    output.Write(s.Replace("\"", "\\\""));
                    output.Write('"');
                }
                else
                {
                    output.Write(s);
                }
                return count;
            }

            var customFormatter = (ICustomFormatter?) formatProvider?.GetFormat(typeof(ICustomFormatter));
            if (customFormatter != null)
            {
                output.Write(customFormatter.Format(format, value, formatProvider));
                return count;
            }

            if (value is IFormattable f)
            {
                output.Write(f.ToString(format, formatProvider ?? CultureInfo.InvariantCulture));
                return count;
            }
            else
            {
                output.Write(value.ToString());
                return count;
            }
        }
    }

    int RenderSequenceValue(SequenceValue sequence, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        var count = 0;
        var elements = sequence.Elements;

        using (_tertiary.Set(output, ref count))
            output.Write('[');
        var allButLast = elements.Count - 1;
        for (var i = 0; i < allButLast; ++i)
        {
            count += Render(elements[i], output, format, formatProvider);
            using (_tertiary.Set(output, ref count))
                output.Write(", ");
        }

        if (elements.Count > 0)
            count += Render(elements[elements.Count - 1], output, format, formatProvider);

        using (_tertiary.Set(output, ref count))
            output.Write(']');

        return count;
    }

    int RenderProperty(LogEventProperty property, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        var count = 0;
        using (_name.Set(output, ref count))
            output.Write(property.Name);
        using (_tertiary.Set(output, ref count))
            output.Write(": ");
        count += Render(property.Value, output, null, formatProvider);
        return count;
    }

    int RenderStructureValue(StructureValue structure, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        var count = 0;
        var properties = structure.Properties;

        if (structure.TypeTag != null)
        {
            using (_name.Set(output, ref count))
                output.Write(structure.TypeTag);
            output.Write(' ');
        }
        using (_tertiary.Set(output, ref count))
            output.Write("{ ");
        var allButLast = properties.Count - 1;
        for (var i = 0; i < allButLast; i++)
        {
            var property = properties[i];
            count += RenderProperty(property, output, format, formatProvider);
            using (_tertiary.Set(output, ref count))
                output.Write(", ");
        }

        if (properties.Count > 0)
        {
            var last = properties[properties.Count - 1];
            count += RenderProperty(last, output, format, formatProvider);
        }

        using (_tertiary.Set(output, ref count))
            output.Write(" }");

        return count;
    }

    int RenderDictionaryValue(DictionaryValue dictionary, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        var count = 0;

        using (_tertiary.Set(output, ref count))
            output.Write('[');
        var delim = "(";
        foreach (var kvp in dictionary.Elements)
        {
            using (_tertiary.Set(output, ref count))
                output.Write(delim);
            delim = ", (";
            count += RenderScalarValue(kvp.Key, output, null, formatProvider);
            using (_tertiary.Set(output, ref count))
                output.Write(": ");
            count += Render(kvp.Value, output, null, formatProvider);
            using (_tertiary.Set(output, ref count))
                output.Write(")");
        }

        using (_tertiary.Set(output, ref count))
            output.Write(']');

        return count;
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="state"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    protected override int VisitScalarValue(ValueFormatterVisitorState state, ScalarValue scalar)
    {
        return RenderScalarValue(scalar, state.Output, state.Format, state.FormatProvider);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="state"></param>
    /// <param name="sequence"></param>
    /// <returns></returns>
    protected override int VisitSequenceValue(ValueFormatterVisitorState state, SequenceValue sequence)
    {
        return RenderSequenceValue(sequence, state.Output, state.Format, state.FormatProvider);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="state"></param>
    /// <param name="structure"></param>
    /// <returns></returns>
    protected override int VisitStructureValue(ValueFormatterVisitorState state, StructureValue structure)
    {
        return RenderStructureValue(structure, state.Output, state.Format, state.FormatProvider);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="state"></param>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    protected override int VisitDictionaryValue(ValueFormatterVisitorState state, DictionaryValue dictionary)
    {
        return RenderDictionaryValue(dictionary, state.Output, state.Format, state.FormatProvider);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class ValueFormatterVisitorState
    {
        /// <summary>
        /// TODO
        /// </summary>
        public ValueFormatterVisitorState(TextWriter output, string? format, IFormatProvider? formatProvider)
        {
            Output = output;
            Format = format;
            FormatProvider = formatProvider;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public TextWriter Output { get; }
        /// <summary>
        /// TODO
        /// </summary>
        public string? Format { get; }
        /// <summary>
        /// TODO
        /// </summary>
        public IFormatProvider? FormatProvider { get; }
    }
}