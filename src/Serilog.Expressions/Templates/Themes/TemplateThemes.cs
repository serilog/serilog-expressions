using System.Collections.Generic;

namespace Serilog.Templates.Themes
{
    static class TemplateThemes
    {
        public static TemplateTheme Literate { get; } = new(
            new Dictionary<TemplateThemeStyle, string>
            {
                [TemplateThemeStyle.Text] = "\x1b[38;5;0015m",
                [TemplateThemeStyle.SecondaryText] = "\x1b[38;5;0007m",
                [TemplateThemeStyle.TertiaryText] = "\x1b[38;5;0008m",
                [TemplateThemeStyle.Invalid] = "\x1b[38;5;0011m",
                [TemplateThemeStyle.Null] = "\x1b[38;5;0027m",
                [TemplateThemeStyle.Name] = "\x1b[38;5;0007m",
                [TemplateThemeStyle.String] = "\x1b[38;5;0045m",
                [TemplateThemeStyle.Number] = "\x1b[38;5;0200m",
                [TemplateThemeStyle.Boolean] = "\x1b[38;5;0027m",
                [TemplateThemeStyle.Scalar] = "\x1b[38;5;0085m",
                [TemplateThemeStyle.LevelVerbose] = "\x1b[38;5;0007m",
                [TemplateThemeStyle.LevelDebug] = "\x1b[38;5;0007m",
                [TemplateThemeStyle.LevelInformation] = "\x1b[38;5;0015m",
                [TemplateThemeStyle.LevelWarning] = "\x1b[38;5;0011m",
                [TemplateThemeStyle.LevelError] = "\x1b[38;5;0015m\x1b[48;5;0196m",
                [TemplateThemeStyle.LevelFatal] = "\x1b[38;5;0015m\x1b[48;5;0196m",
            });

        public static TemplateTheme Grayscale { get; } = new(
            new Dictionary<TemplateThemeStyle, string>
            {
                [TemplateThemeStyle.Text] = "\x1b[37;1m",
                [TemplateThemeStyle.SecondaryText] = "\x1b[37m",
                [TemplateThemeStyle.TertiaryText] = "\x1b[30;1m",
                [TemplateThemeStyle.Invalid] = "\x1b[37;1m\x1b[47m",
                [TemplateThemeStyle.Null] = "\x1b[1m\x1b[37;1m",
                [TemplateThemeStyle.Name] = "\x1b[37m",
                [TemplateThemeStyle.String] = "\x1b[1m\x1b[37;1m",
                [TemplateThemeStyle.Number] = "\x1b[1m\x1b[37;1m",
                [TemplateThemeStyle.Boolean] = "\x1b[1m\x1b[37;1m",
                [TemplateThemeStyle.Scalar] = "\x1b[1m\x1b[37;1m",
                [TemplateThemeStyle.LevelVerbose] = "\x1b[30;1m",
                [TemplateThemeStyle.LevelDebug] = "\x1b[30;1m",
                [TemplateThemeStyle.LevelInformation] = "\x1b[37;1m",
                [TemplateThemeStyle.LevelWarning] = "\x1b[37;1m\x1b[47m",
                [TemplateThemeStyle.LevelError] = "\x1b[30m\x1b[47m",
                [TemplateThemeStyle.LevelFatal] = "\x1b[30m\x1b[47m",
            });

        public static TemplateTheme Code { get; } = new(
            new Dictionary<TemplateThemeStyle, string>
            {
                [TemplateThemeStyle.Text] = "\x1b[38;5;0253m",
                [TemplateThemeStyle.SecondaryText] = "\x1b[38;5;0246m",
                [TemplateThemeStyle.TertiaryText] = "\x1b[38;5;0242m",
                [TemplateThemeStyle.Invalid] = "\x1b[33;1m",
                [TemplateThemeStyle.Null] = "\x1b[38;5;0038m",
                [TemplateThemeStyle.Name] = "\x1b[38;5;0081m",
                [TemplateThemeStyle.String] = "\x1b[38;5;0216m",
                [TemplateThemeStyle.Number] = "\x1b[38;5;151m",
                [TemplateThemeStyle.Boolean] = "\x1b[38;5;0038m",
                [TemplateThemeStyle.Scalar] = "\x1b[38;5;0079m",
                [TemplateThemeStyle.LevelVerbose] = "\x1b[37m",
                [TemplateThemeStyle.LevelDebug] = "\x1b[37m",
                [TemplateThemeStyle.LevelInformation] = "\x1b[37;1m",
                [TemplateThemeStyle.LevelWarning] = "\x1b[38;5;0229m",
                [TemplateThemeStyle.LevelError] = "\x1b[38;5;0197m\x1b[48;5;0238m",
                [TemplateThemeStyle.LevelFatal] = "\x1b[38;5;0197m\x1b[48;5;0238m",
            });
    }
}