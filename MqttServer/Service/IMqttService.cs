internal interface IMqttService : IDisposable
{
    public Task StartServer();
    public Task StopServer();
    public Task PublishMessage(string clientId, string topic, string message); 
}