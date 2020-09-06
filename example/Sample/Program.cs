using Serilog;

namespace Sample
{
    public static class Program
    {
        public static void Main()
        {
            const string expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

            using var log = new LoggerConfiguration()
                .Enrich.WithProperty("AppId", 10)
                .Filter.ByIncludingOnly(expr)
                .WriteTo.Console()
                .CreateLogger();

            log.Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Information("Cart contains {@Items}", new[] { "Apricots" });
            log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });
        }
    }
}
