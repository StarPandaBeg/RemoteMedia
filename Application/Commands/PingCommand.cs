
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Commands {
    class PingCommand : ICommand {

        private byte[] _data;

        public PingCommand(string text) {
            var message = Encoding.UTF8.GetBytes(text);
            _data = message.Prepend((byte)CommandStatus.OK).ToArray();
        }

        public PingCommand() {
            _data = CommandStatus.OK.GetStatus();
        }

        public Task<byte[]> Execute() {
            return Task.FromResult(_data);
        }
    }
}
