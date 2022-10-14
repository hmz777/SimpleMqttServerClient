using System.Text;
using MqttClient.AppConfiguration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Serilog;

namespace MqttClient.Service;

public class MqttClientService : IMqttService
{
    private bool _disposedValue;
    private readonly IManagedMqttClient? _managedMqttClient;
    private readonly ManagedMqttClientOptions? _managedMqttClientOptions;

    public MqttClientService()
    {
        try
        {
            var mqttFactory = new MqttFactory();
            _managedMqttClient = mqttFactory.CreateManagedMqttClient();

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(Config.ClientParameters.BrokerHost, Config.ClientParameters.BrokerPort)
                .WithClientId(Config.ClientParameters.ClientId)
                .WithCredentials(Config.ClientParameters.Username, Config.ClientParameters.Password)
                .WithCleanSession(Config.ClientParameters.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(Config.ClientParameters.KeepAlive));

            if (Config.ClientParameters.WithWill)
            {
                mqttClientOptionsBuilder = mqttClientOptionsBuilder
                    .WithWillTopic(Config.ClientParameters.LastWillTopic)
                    .WithWillPayload(Config.ClientParameters.LastWillMessage)
                    .WithWillQualityOfServiceLevel((MqttQualityOfServiceLevel)Config.ClientParameters.LastWillQos);
            }

            var mqttClientOptions = mqttClientOptionsBuilder.Build();

            _managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(mqttClientOptions)
                .Build();

            _managedMqttClient.ConnectedAsync += args =>
            {
                Log.Information("Client connected to broker:{Broker}", Config.ClientParameters.BrokerHost);
                return Task.CompletedTask;
            };

            _managedMqttClient.DisconnectedAsync += args =>
            {
                Log.Information("Client disconnected from broker:{Broker}", Config.ClientParameters.BrokerHost);
                return Task.CompletedTask;
            };

            _managedMqttClient.ConnectingFailedAsync += args =>
            {
                Log.Error("Connection to broker:{Broker} failed!", Config.ClientParameters.BrokerHost);
                return Task.CompletedTask;
            };

            _managedMqttClient.ApplicationMessageReceivedAsync += args =>
            {
                Log.Information(@"Received message with 
                                                Topic:{Topic}
                                                Payload:{Payload}
                                                Qos:{Qos}
                                                Retain:{Retain}
                                                Dup:{Dup}"
                    , args.ApplicationMessage.Topic,
                    Encoding.UTF8.GetString(args.ApplicationMessage.Payload),
                    args.ApplicationMessage.QualityOfServiceLevel,
                    args.ApplicationMessage.Retain,
                    args.ApplicationMessage.Dup);

                return Task.CompletedTask;
            };

            Log.Information("MQTT client with id:{ClientId} initialized successfully",
                Config.ClientParameters.ClientId);
        }
        catch (Exception e)
        {
#if DEBUG
            Log.Error(e, "An exception occured while initializing the MQTT client");
#else
            Log.Error("An exception occured while initializing the MQTT client, with error message:{Message}", e.Message);
#endif
        }
    }

    public async Task Connect()
    {
        await _managedMqttClient.StartAsync(_managedMqttClientOptions);

        Log.Information("Client started");
    }

    public async Task Disconnect()
    {
        await _managedMqttClient.StopAsync();

        Log.Information("Client stopped");
    }

    public async Task Publish(string? topic, string? payload)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(topic, nameof(topic));
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            await _managedMqttClient.EnqueueAsync(topic, payload,
                (MqttQualityOfServiceLevel)Config.ClientParameters.Qos,
                Config.ClientParameters.Retain);

            Log.Information("Enqueued message with topic:{Topic} and payload:{Payload}", topic, payload);
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

    public async Task Subscribe(string? topic)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(topic, nameof(topic));
            
            if (_managedMqttClient?.IsConnected ?? false)
            {
                var mqttFactory = new MqttFactory();

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => { f.WithTopic(topic); })
                    .Build();

                await _managedMqttClient.SubscribeAsync(topic, (MqttQualityOfServiceLevel)Config.ClientParameters.Qos);
            }

            Log.Information("Client subscribed to topic:{Topic}", topic);
        }
        catch (Exception e)
        {
#if DEBUG
            Log.Error(e, "An exception occured while subscribing to a topic");
#else
            Log.Error("An exception occured while subscribing to a topic, with error message:{Message}", e.Message);
#endif
        }
    }

    public bool IsConnected => _managedMqttClient?.IsConnected ?? false;

    void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _managedMqttClient.Dispose();
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