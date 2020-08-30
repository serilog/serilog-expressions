using Serilog;

namespace Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var expr = "@Level = 'Information' and AppId is not null and Items[?] like 'C%'";

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("AppId", 10)
                .Filter.ByIncludingOnly(expr)
                .WriteTo.LiterateConsole()
                .CreateLogger();

            Log.Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            Log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            Log.Information("Cart contains {@Items}", new[] { "Apricots" });
            Log.Information("Cart contains {@Items}", new[] { "Peanuts", "Chocolate" });

            Log.CloseAndFlush();
        }
    }
}
