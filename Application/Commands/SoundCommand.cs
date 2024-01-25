
using RemoteMedia.Application.Util;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Commands {
    class SoundCommand : ICommand {

        private int _value;

        public SoundCommand(int value) {
            _value = value;
        }

        public Task<byte[]> Execute() {
            if (_value < 0 || _value > 100) {
                return Task.FromResult(CommandStatus.FAIL.GetStatus());
            }

            SystemAudio.SetVolume(_value);
            return Task.FromResult(CommandStatus.OK.GetStatus());
        }
    }
}
