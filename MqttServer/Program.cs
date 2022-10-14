using Common.AppLogging;
using MqttServer.Service;
using Serilog;

// Create application logger
AppLogger.CreateLogger();

Log.Information("MQTT Broker - Hamzi Alsheikh");

IMqttService mqttServerService = null;

mqttServerService = new MqttServerService();
await mqttServerService.StartServer();

Log.Information("Enter q to stop the broker");

while (Console.ReadLine() != "q");

Log.Information("Stopping MQTT broker...");

await mqttServerService.StopServer();
mqttServerService.Dispose();
AppLogger.Dispose();