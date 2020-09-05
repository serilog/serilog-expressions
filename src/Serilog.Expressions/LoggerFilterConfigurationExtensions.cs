using System;
using Serilog.Configuration;
using Serilog.Expressions;
using Serilog.Expressions.Runtime;

// ReSharper disable UnusedMember.Global

namespace Serilog
{
    /// <summary>
    /// Extends logger filter configuration with methods for filtering with expressions.
    /// </summary>
    public static class LoggerFilterConfigurationExtensions
    {
        /// <summary>
        /// Include only log events that match the provided expression.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="expression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByIncludingOnly(this LoggerFilterConfiguration loggerFilterConfiguration, string expression)
        {
            if (loggerFilterConfiguration == null) throw new ArgumentNullException(nameof(loggerFilterConfiguration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var compiled = SerilogExpression.Compile(expression);
            return loggerFilterConfiguration.ByIncludingOnly(e => Coerce.True(compiled(e)));
        }

        /// <summary>
        /// Exclude log events that match the provided expression.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="expression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByExcluding(this LoggerFilterConfiguration loggerFilterConfiguration, string expression)
        {
            if (loggerFilterConfiguration == null) throw new ArgumentNullException(nameof(loggerFilterConfiguration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var compiled = SerilogExpression.Compile(expression);
            return loggerFilterConfiguration.ByExcluding(e => Coerce.True(compiled(e)));
        }

        /// <summary>
        /// Use a <see cref="LoggingFilterSwitch"/> to dynamically control filtering.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="switch">A <see cref="LoggingFilterSwitch"/> that can be used to dynamically control
        /// log filtering.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ControlledBy(this LoggerFilterConfiguration loggerFilterConfiguration, LoggingFilterSwitch @switch)
        {
            if (loggerFilterConfiguration == null) throw new ArgumentNullException(nameof(loggerFilterConfiguration));
            if (@switch == null) throw new ArgumentNullException(nameof(@switch));

            return loggerFilterConfiguration.With(@switch);
        }
    }
}
