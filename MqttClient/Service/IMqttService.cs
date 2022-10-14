namespace MqttClient.Service;

internal interface IMqttService : IDisposable
{
    public Task Connect();
    public Task Disconnect();
    public Task Publish(string? topic, string? payload);
    public Task Subscribe(string? topic);
    public bool IsConnected { get; }
}