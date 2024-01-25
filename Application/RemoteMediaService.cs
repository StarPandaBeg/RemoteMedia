using RemoteMedia.Application.Client;
using RemoteMedia.Application.Util;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace RemoteMedia.Application {
    public partial class RemoteMediaService : ServiceBase {

        private readonly IClient _client;

        public RemoteMediaService() {
            InitializeComponent();

            _client = new MQTTClient(eventLog);
        }

        protected override void OnStart(string[] args) {
            MediaManagerHelper.Start();
            _client.Start();
        }

        protected override void OnStop() {
            _client.Stop();
            MediaManagerHelper.Stop();
        }
    }
}
