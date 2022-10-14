using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MqttServer.Service;
using Serilog;

namespace MqttServer.Service;

internal class MqttServerService : IMqttService
{
    private readonly MQTTnet.Server.MqttServer? _mqttServer;
    private bool _disposedValue;

    public MqttServerService()
    {
        try
        {
            var serverFactory = new MqttFactory(new MqttLogger());
            var mqttServerOptions = serverFactory
                .CreateServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithPersistentSessions()
                .Build();

            _mqttServer = serverFactory.CreateMqttServer(mqttServerOptions);

            _mqttServer.ValidatingConnectionAsync += ValidateConnection;

            _mqttServer.ClientConnectedAsync += args =>
            {
                Log.Information("Client connected with id:{ClientId}", args.ClientId);
                return Task.CompletedTask;
            };

            _mqttServer.ClientDisconnectedAsync += args =>
            {
                Log.Information("Client with id:{ClientId} disconnected", args.ClientId);
                return Task.CompletedTask;
            };

            _mqttServer.ValidatingConnectionAsync += args =>
            {
                Log.Information("Validating client with id:{ClientId}", args.ClientId);
                return Task.CompletedTask;
            };

            Log.Information("MQTT Server initialized successfully");
        }
        catch (Exception e)
        {
#if DEBUG
            Log.Error(e, "An exception occured while initializing the MQTT server");
#else
            Log.Error("An exception occured while initializing the MQTT server, with error message:{Message}", e.Message);
#endif
        }
    }

    public async Task PublishMessage(string clientId, string topic, string message)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(topic, nameof(topic));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            var brokerMessage = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(message).Build();

            await _mqttServer.InjectApplicationMessage(
                new InjectedMqttApplicationMessage(brokerMessage)
                {
                    SenderClientId = clientId
                });

            Log.Information("Published message with {Topic} from client with id {ClientId}",
                topic, clientId);
        }
        catch (Exception e)
        {
#if DEBUG
            Log.Error(e, "An exception occured while publishing a message");
#else
            Log.Error("An exception occured while publishing a message, with error message:{Message}", e.Message);
#endif
        }
    }

    public async Task StartServer()
    {
        await _mqttServer.StartAsync();
    }

    public async Task StopServer()
    {
        await _mqttServer.StopAsync();
    }

    private Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
    {
        // if (eventArgs.ClientId != "ValidClientId")
        // {
        //     eventArgs.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
        // }

        if (eventArgs.UserName != "HMZ")
        {
            eventArgs.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        }

        if (eventArgs.Password != "ZMH")
        {
            eventArgs.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        }

        return Task.CompletedTask;
    }

    void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _mqttServer.ValidatingConnectionAsync -= ValidateConnection;
                _mqttServer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}