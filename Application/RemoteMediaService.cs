using RemoteMedia.Application.Client;
using RemoteMedia.Application.Util;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace RemoteMedia.Application {
    public partial class RemoteMediaService : ServiceBase {

        private readonly IClient _client;

        public RemoteMediaService() {
            InitializeComponent();

            _client = new MQTTClient(eventLog);
        }

        protected override void OnStart(string[] args) {
            try {
                MediaManagerHelper.Start();
                _client.Start().Wait();
            } catch (Exception e) {
                eventLog.WriteEntry("An exception thrown during service execution: " + e, EventLogEntryType.Error);
                Stop();
            }
        }

        protected override void OnStop() {
            try {
                _client.Stop().Wait();
                MediaManagerHelper.Stop();
            } catch (Exception e) {
                eventLog.WriteEntry("An exception thrown during service shutdown: " + e, EventLogEntryType.Error);
            }
        }
    }
}
