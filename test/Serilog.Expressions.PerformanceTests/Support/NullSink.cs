using Serilog.Core;
using Serilog.Events;

namespace Serilog.Expressions.PerformanceTests.Support
{
    public class NullSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
        }
    }
}
