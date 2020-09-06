using System;
using Serilog;
using Serilog.Debugging;
using Serilog.Templates;

namespace Sample
{
    public class Program
    {
        public static void Main()
        {
            SelfLog.Enable(Console.Error);
            
            const string expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

            using var log = new LoggerConfiguration()
                .Enrich.WithProperty("AppId", 10)
                .Enrich.WithComputed("FirstItem", "Items[0]")
                .Enrich.WithComputed("SourceContext", "coalesce(substring(SourceContext, lastindexof(SourceContext, '.') + 1), SourceContext, '<no source>')")
                .Filter.ByIncludingOnly(expr)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3} ({SourceContext})] {Message:lj} (first item is {FirstItem}){NewLine}{Exception}")
                .WriteTo.Console(new OutputTemplate(
                    "[{@Timestamp} {@Level} ({SourceContext})] {@Message} (first item is {Items[0]})\n{@Exception}"))
                    .CreateLogger();

            log.ForContext<Program>().Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Information("Cart contains {@Items}", new[] { "Apricots" });
            log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });
        }
    }
}
