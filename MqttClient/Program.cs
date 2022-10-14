using CommandLine;
using Common.AppLogging;
using MqttClient.AppConfiguration;
using MqttClient.Service;
using MqttClient.Validation;
using Serilog;

// Create application logger
AppLogger.CreateLogger();

var result = Parser.Default.ParseArguments<ClientParameters>(args);

if (result.Tag == ParserResultType.NotParsed)
    return;

var parametersValidator = new ParametersValidator();
var validationResult = parametersValidator.Validate(result.Value);

if (validationResult.IsValid == false)
    return;

Config.ClientParameters = result.Value;

Log.Information("MQTT Client - Hamzi Alsheikh");

IMqttService mqttClientService = new MqttClientService();
await mqttClientService.Connect();

Log.Information("Waiting for a connection to be established...");
while (mqttClientService.IsConnected == false)
    await Task.Delay(1000);

string? input = null;
string? topicToPublish = null;
string? payload = null;
string? topicToSubscribe = null;
bool canLoop = true;

while (canLoop)
{
    if (mqttClientService.IsConnected == false)
    {
        Log.Error("Connection lost");
        break;
    }

    Log.Information($"Enter p for publish, s for subscribe or q to exit");
    input = Console.ReadLine();

    switch (input)
    {
        case "p":
            Log.Information($"Enter a message topic:");
            topicToPublish = Console.ReadLine();
            Log.Information($"Enter a message payload:");
            payload = Console.ReadLine();

            await mqttClientService.Publish(topicToPublish, payload);
            break;
        case "s":
            Log.Information($"Enter a topic to subscribe to:");
            topicToSubscribe = Console.ReadLine();
            await mqttClientService.Subscribe(topicToSubscribe);
            break;
        case "q":
            Log.Information("Client ended by user");
            canLoop = false;
            break;
        default:
            Log.Error("No valid option selected!");
            break;
    }
}

Log.Information("Exiting...");

await mqttClientService.Disconnect();
mqttClientService.Dispose();
AppLogger.Dispose();