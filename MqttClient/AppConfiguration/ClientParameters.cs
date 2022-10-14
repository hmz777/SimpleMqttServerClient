using System.ComponentModel.DataAnnotations;
using CommandLine;

namespace MqttClient.AppConfiguration;

public class ClientParameters
{
    [Option("clientId", Required = true, HelpText = "The client id")]
    public string ClientId { get; set; }

    [Option("brokerHost", Required = false, HelpText = "The broker address",
        Default = "127.0.0.1")]
    public string BrokerHost { get; set; }

    [Option("brokerPort", Required = false, HelpText = "The broker port", Default = 1883)]
    public int BrokerPort { get; set; }

    [Option("cleanSession", Required = false, HelpText = "Indicates if the session will be clean or persistent")]

    public bool CleanSession { get; set; }

    [Option("keepAlive", Required = false,
        HelpText = "Seconds to keep the client alive between messages", Default = 30)]
    public int KeepAlive { get; set; }

    [Option("ssl", Required = false, HelpText = "Indicates the use of SSL", Default = false)]

    public bool SSL { get; set; }

    [Option("username", Required = true, HelpText = "Username for authenticating with the broker")]
    public string Username { get; set; }

    [Option("password", Required = true, HelpText = "Password for authenticating with the broker")]
    public string Password { get; set; }

    [Option("retain", Required = false, HelpText = "Indicates if messages will be retained", Default = false)]
    public bool Retain { get; set; }

    [Option("qos", Required = false, HelpText = "Quality of service level", Default = 0)]
    public int Qos { get; set; }

    [Option("withWill", Required = false, HelpText = "Indicates if there will be a will message")]
    public bool WithWill { get; set; }

    [Option("lastWillTopic", Required = false, HelpText = "The topic of the last will message")]
    public string LastWillTopic { get; set; }

    [Option("lastWillQos", Required = false, HelpText = "The quality of service level for the last will message",
        Default = 0)]
    public int LastWillQos { get; set; }

    [Option("lastWillMessage", Required = false, HelpText = "The message body of the last will message")]
    public string LastWillMessage { get; set; }
}