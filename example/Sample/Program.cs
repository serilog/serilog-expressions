using Serilog;

namespace Sample
{
    public class Program
    {
        public static void Main()
        {
            const string expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

            using var log = new LoggerConfiguration()
                .Enrich.WithProperty("AppId", 10)
                .Enrich.WithComputed("FirstItem", "Items[0]")
                .Enrich.WithComputed("SourceContext", "coalesce(substring(SourceContext, lastindexof(SourceContext, '.') + 1), SourceContext, '<no source>')")
                .Filter.ByIncludingOnly(expr)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3} ({SourceContext})] {Message:lj} (first item is {FirstItem}){NewLine}{Exception}")
                .CreateLogger();

            log.ForContext<Program>().Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Information("Cart contains {@Items}", new[] { "Apricots" });
            log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });
        }
    }
}
