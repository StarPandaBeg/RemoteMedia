
using System.Configuration;

namespace RemoteMedia.Application.Config {
    public class MqttConfig : ConfigurationSection {

        [ConfigurationProperty("encKey", IsRequired = true)] public string EncryptionKey => (string)this["encKey"];
        [ConfigurationProperty("brokerHost", IsRequired = true)] public string Host => (string)this["brokerHost"];
        [ConfigurationProperty("listenTopic", IsRequired = true)] public string ListenTopic => (string)this["listenTopic"];
        [ConfigurationProperty("responseTopic", IsRequired = true)] public string ResponseTopic => (string)this["responseTopic"];
        [ConfigurationProperty("pingTopic", DefaultValue = "/ping")] public string PingTopic => (string)this["pingTopic"];
        [ConfigurationProperty("brokerPort", DefaultValue = 1883)] public int Port => (int)this["brokerPort"];
        [ConfigurationProperty("username")] public string Username => (string)this["username"];
        [ConfigurationProperty("password")] public string Password => (string)this["password"];

        public bool HasCredentials => !string.IsNullOrEmpty(Username);

        public static MqttConfig LoadConfig() {
            return (MqttConfig)ConfigurationManager.GetSection("mqttConfig");
        }
    }
}
