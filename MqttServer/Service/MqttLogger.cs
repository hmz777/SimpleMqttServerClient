using MQTTnet.Diagnostics;
using Serilog;
using Serilog.Events;

namespace MqttServer.Service;

public class MqttLogger : IMqttNetLogger
{
    private readonly object _consoleSyncRoot = new();

    public bool IsEnabled => true;

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters,
        Exception? exception)
    {
        var loggerLogLevel = logLevel switch
        {
            MqttNetLogLevel.Verbose => LogEventLevel.Verbose,
            MqttNetLogLevel.Info => LogEventLevel.Information,
            MqttNetLogLevel.Warning => LogEventLevel.Warning,
            MqttNetLogLevel.Error => LogEventLevel.Error,
            _ => LogEventLevel.Information
        };

        if (parameters?.Length > 0)
        {
            message = string.Format(message, parameters);
        }

        lock (_consoleSyncRoot)
        {
            Log.Write(loggerLogLevel, message);

            if (exception != null)
            {
#if DEBUG
                Log.Error(exception, "Exception occured in MQTT service");
#else
                Log.Error("Exception occured in MQTT service, with error message:{Message}", exception.Message);
#endif
            }
        }
    }
}