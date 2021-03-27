using System;
using System.Collections.Generic;
using Serilog.Events;
using Xunit.Sdk;

namespace Serilog.Expressions.Tests.Support
{
    static class Some
    {
        public static LogEvent InformationEvent(string messageTemplate = "Hello, world!", params object?[] propertyValues)
        {
            return LogEvent(LogEventLevel.Information, messageTemplate, propertyValues);
        }

        public static LogEvent WarningEvent(string messageTemplate = "Hello, world!", params object?[] propertyValues)
        {
            return LogEvent(LogEventLevel.Warning, messageTemplate, propertyValues);
        }

        public static LogEvent LogEvent(LogEventLevel level, string messageTemplate = "Hello, world!", params object?[] propertyValues)
        {
            var log = new LoggerConfiguration().CreateLogger();
            MessageTemplate template;
            IEnumerable<LogEventProperty> properties;
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            if (!log.BindMessageTemplate(messageTemplate, propertyValues, out template, out properties))
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
            {
                throw new XunitException("Template could not be bound.");
            }
            return new LogEvent(DateTimeOffset.Now, level, null, template, properties);
        }

        public static object AnonymousObject()
        {
            return new {A = 42};
        }

        public static LogEventPropertyValue LogEventPropertyValue()
        {
            return new ScalarValue(AnonymousObject());
        }
    }
}
