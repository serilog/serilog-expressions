using System;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;

namespace Serilog.Pipeline
{
    class ComputedPropertyEnricher : ILogEventEnricher
    {
        readonly string _propertyName;
        readonly CompiledExpression _computeValue;

        public ComputedPropertyEnricher(string propertyName, CompiledExpression computeValue)
        {
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _computeValue = computeValue ?? throw new ArgumentNullException(nameof(computeValue));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var value = _computeValue(logEvent);
            if (value != null)
                logEvent.AddOrUpdateProperty(new LogEventProperty(_propertyName, value));
        }
    }
}
