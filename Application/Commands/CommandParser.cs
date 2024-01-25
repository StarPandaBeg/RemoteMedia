using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteMedia.Application.Commands {
    class CommandParser {
        private Dictionary<byte, Func<byte[], ICommand>> _registry = new Dictionary<byte, Func<byte[], ICommand>>();

        public CommandParser() {
            Register(0x00, CreatePingCommand);
            Register(0x01, CreateActionCommand);
            Register(0x02, CreateSoundCommand);
            Register(0x03, CreateInfoCommand);
        }

        public ICommand Parse(byte[] data) {
            if (data == null || data.Length == 0) {
                return new ErrorCommand("No payload provided");
            }

            if (_registry.TryGetValue(data[0], out var factory)) {
                return factory(data.Skip(1).ToArray());
            }
            return new ErrorCommand("Unknown command");
        }

        public void Register(byte id, Func<byte[], ICommand> factory) {
            _registry[id] = factory;
        }

        private ICommand CreatePingCommand(byte[] data) {
            return new PingCommand();
        }

        private ICommand CreateActionCommand(byte[] data) {
            if (data.Length == 0 || !Enum.IsDefined(typeof(ActionCommand.Action), (int)data[0])) {
                return new ErrorCommand("Unknown action");
            }

            var action = (int)data[0];
            return new ActionCommand((ActionCommand.Action)action);
        }

        private ICommand CreateSoundCommand(byte[] data) {
            if (data.Length < 4) {
                return new ErrorCommand("No sound value");
            }

            var num = data.Take(4);
            if (BitConverter.IsLittleEndian) num = num.Reverse();

            var value = BitConverter.ToInt32(num.ToArray(), 0);
            Console.WriteLine(value);
            return new SoundCommand(value);
        }

        private ICommand CreateInfoCommand(byte[] data) {
            return new InfoCommand();
        }
    }
}
