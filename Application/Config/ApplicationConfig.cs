using System;
using System.Configuration;

namespace RemoteMedia.Application.Config {
    static class ApplicationConfig {

        public static string MqttHost => GetConfigurationValue("MQTT_HOST", true);
        public static int MqttPort => Convert.ToInt32(GetConfigurationValue("MQTT_PORT", false, "1883"));

        public static string MqttListenTopic => GetConfigurationValue("MQTT_LISTEN_TOPIC", false, "/remotemedia/control");
        public static string MqttResponseTopic => GetConfigurationValue("MQTT_RESPONSE_TOPIC", false, "/remotemedia/response");
        public static string MqttPingTopic => GetConfigurationValue("MQTT_PING_TOPIC", false, "/remotemedia/ping");

        public static bool MqttHasCredentials => GetBooleanConfigurationValue("MQTT_USE_CREDENTIALS", false, false);
        public static string MqttUser => GetConfigurationValue("MQTT_USERNAME", MqttHasCredentials);
        public static string MqttPassword => GetConfigurationValue("MQTT_PASSWORD", MqttHasCredentials);

        public static string EncryptionKey => GetConfigurationValue("ENCRYPTION_KEY", true);

        private static string GetConfigurationValue(string key, bool required = false, string fallback = null) {
            string value = ConfigurationManager.AppSettings[key];
            
            if (string.IsNullOrEmpty(value)) {
                if (required) throw new ConfigurationErrorsException($"Missing configuration for key '{key}'.");
                return fallback;
            }
            return value;
        }

        private static bool GetBooleanConfigurationValue(string key, bool required = false, bool fallback = false) {
            string value = GetConfigurationValue(key, required);

            if (string.IsNullOrEmpty(value)) {
                return fallback;
            }
            return Convert.ToBoolean(value);
        }
    }
}
