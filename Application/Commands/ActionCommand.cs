using RemoteMedia.Application.Util;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Commands {
    class ActionCommand : ICommand {

        public enum Action {
            PLAY_PAUSE = 0x00,
            PREV = 0x01,
            NEXT = 0x02,
        }

        private Action _action;

        public ActionCommand(Action action) {
            _action = action;
        }

        public async Task<byte[]> Execute() {
            Task<bool> task;
            switch (_action) {
                case Action.PLAY_PAUSE:
                    task = MediaManagerHelper.PlayPause();
                    break;
                case Action.PREV:
                    task = MediaManagerHelper.Prev();
                    break;
                case Action.NEXT:
                    task = MediaManagerHelper.Next();
                    break;
                default:
                    task = new Task<bool>(() => false);
                    break;
            }

            var result = await task;
            var status = result ? CommandStatus.OK : CommandStatus.FAIL;
            return status.GetStatus();
        }
    }
}
