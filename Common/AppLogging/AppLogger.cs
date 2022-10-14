using Serilog;
using Serilog.Events;

namespace Common.AppLogging;

public static class AppLogger
{
    public static void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
    }

    public static void Dispose()
    {
        Log.CloseAndFlush();
    }
}