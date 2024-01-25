using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Client {
    public interface IClient {
        Task Start();
        Task Stop();
    }
}
