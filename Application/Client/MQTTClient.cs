using RemoteMedia.Application.Commands;
using RemoteMedia.Application.Config;
using RemoteMedia.Application.Security;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RemoteMedia.Application.Client {
    class MQTTClient : IClient, IDisposable {

        private IManagedMqttClient _client;
        private CommandParser _parser;
        private MqttConfig _config;
        private EventLog _logger;
        private SecurityUtility _security;

        public MQTTClient(EventLog logger) {
            _logger = logger;

            _parser = new CommandParser();
            _client = BuildClient();
        }

        private async Task OnConnected(MqttClientConnectedEventArgs arg) {
            await SubscribeTopic(_config);
        }

        public async Task Start() {
            _logger.WriteEntry("Trying to start MQTT client");

            _config = MqttConfig.LoadConfig();
            var options = BuildClientOptions(_config);

            _client.ApplicationMessageReceivedAsync += MessageReceived;
            _client.ConnectedAsync += OnConnected;
            _security = new SecurityUtility(Encoding.UTF8.GetBytes(_config.EncryptionKey));

            await _client.StartAsync(options);
            new Thread(new ThreadStart(this.PingLoop)).Start();

            _logger.WriteEntry("MQTT client started");
        }

        public async Task Stop() {
            _logger.WriteEntry("Trying to stop MQTT client");

            if (_client == null) {
                _logger.WriteEntry("MQTT client stopped");
                return;
            }

            _client.ApplicationMessageReceivedAsync -= MessageReceived;
            _client.ConnectedAsync -= OnConnected;
            if (_client.IsStarted) { 
                await _client.StopAsync(); 
            }
            _logger.WriteEntry("MQTT client stopped");
        }

        private async Task MessageReceived(MqttApplicationMessageReceivedEventArgs arg) {
            _logger.WriteEntry("Received new message");

            var data = arg.ApplicationMessage.PayloadSegment.Array;
            var command = GetCommand(data);

            byte[] output = null;
            try {
                output = await command.Execute();
            } catch (Exception e) {
                _logger.WriteEntry($"Error during command execution. Exception thrown: {e}", EventLogEntryType.Error);
            }
            if (output == null || output.Length == 0) return;

            output = _security.Sign(output);
            output = _security.Encrypt(output);

            var message = new MqttApplicationMessageBuilder().WithTopic(_config.ResponseTopic).WithPayload(output).Build();
            await _client.EnqueueAsync(message);

            _logger.WriteEntry("Message response added to the queue");
        }

        public void Dispose() {
            Stop().Wait();
        }

        private async Task SubscribeTopic(MqttConfig config) {
            var filterBuilder = new MqttTopicFilterBuilder();
            filterBuilder.WithTopic(config.ListenTopic);

            var filters = new List<MqttTopicFilter> {
                filterBuilder.Build()
            };
            await _client.SubscribeAsync(filters);

            _logger.WriteEntry($"Listening topic '{config.ListenTopic}'");
        }

        private ICommand GetCommand(byte[] input) {
            var data = input;

            try {
                data = _security.Decrypt(data);
                if (!_security.CheckNonce(data, out var p1)) throw new Exception("Message repeated");
                if (!_security.Validate(p1, out var p2)) throw new Exception("Invalid sign");

                return _parser.Parse(p2);
            } catch (Exception e) {
                return new ErrorCommand(e.Message);
            }
        }

        private void PingLoop() {
            while (_client.IsStarted) {
                Thread.Sleep(20000);
                if (!_client.IsConnected) continue;
                _client.EnqueueAsync(
                    new MqttApplicationMessageBuilder().WithTopic(_config.PingTopic).WithPayload("ping").Build()
                );
            }
        }

        private static ManagedMqttClientOptions BuildClientOptions(MqttConfig config) {
            var factory = new MqttFactory();

            var clientOptions = factory.CreateClientOptionsBuilder();
            clientOptions.WithTcpServer(config.Host, config.Port);

            if (config.HasCredentials) {
                clientOptions.WithCredentials(config.Username, config.Password);
            }

            var optionsBuilder = factory.CreateManagedMqttClientOptionsBuilder();
            optionsBuilder.WithAutoReconnectDelay(TimeSpan.FromSeconds(5));
            optionsBuilder.WithClientOptions(clientOptions);

            return optionsBuilder.Build();
        }

        private static IManagedMqttClient BuildClient() {
            var factory = new MqttFactory();
            return factory.CreateManagedMqttClient();
        }
    }
}
