using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Commands {
    class ErrorCommand : ICommand {

        private byte[] _data;

        public ErrorCommand(string text = "Unknown error") {
            var message = Encoding.UTF8.GetBytes(text);
            _data = message.Prepend((byte)CommandStatus.FAIL).ToArray();
        }

        public Task<byte[]> Execute() {
            return Task.FromResult(_data);
        }
    }
}
