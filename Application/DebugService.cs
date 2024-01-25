using System;

#if DEBUG
namespace RemoteMedia.Application {
    public class DebugService : RemoteMediaService, IDisposable {
        public void DebugStart(string[] args) {
            OnStart(args);
        }

        public void DebugStop() {
            OnStop();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            DebugStop();
        }
    }
}
#endif