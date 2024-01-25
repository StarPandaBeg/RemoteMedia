using System.Threading.Tasks;

namespace RemoteMedia.Application.Commands {
    interface ICommand {
        Task<byte[]> Execute();
    }
}
