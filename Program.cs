using RemoteMedia.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace RemoteMedia
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (var service = new DebugService()) {
                service.DebugStart(null);
                Thread.Sleep(Timeout.Infinite);
            }
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new RemoteMediaService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
