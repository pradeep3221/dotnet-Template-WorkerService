using Serilog;

namespace WorkerService.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static ILogger CreateLogger() =>
        new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();
}
