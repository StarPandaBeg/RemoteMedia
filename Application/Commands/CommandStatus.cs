
namespace RemoteMedia.Application.Commands {
    enum CommandStatus {
        EMPTY = 0x00,
        OK = 0x01,
        FAIL = 0x02,
    }

    static class CommandStatusExtension {
        public static byte[] GetStatus(this CommandStatus value) {
            return new byte[] { (byte)value };
        }
    }
}
