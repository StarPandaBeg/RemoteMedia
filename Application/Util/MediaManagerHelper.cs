
using Windows.Media.Control;
using System;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Util {
    static class MediaManagerHelper {

        public static GlobalSystemMediaTransportControlsSessionMediaProperties LastMediaProperties { get; private set; }
        public static GlobalSystemMediaTransportControlsSessionTimelineProperties LastTimelineProperties { get; private set; }
        public static GlobalSystemMediaTransportControlsSessionPlaybackInfo LastPlaybackInfo { get; private set; }

        private static MediaManager _mediaManager;

        private static bool HasSession => _mediaManager.GetFocusedSession() != null;
        private static GlobalSystemMediaTransportControlsSession ControlSession => _mediaManager.GetFocusedSession().ControlSession; 

        public static Task<bool> PlayPause() {
            if (!HasSession) return Task.FromResult(false);
            return ControlSession.TryTogglePlayPauseAsync().AsTask();
        }

        public static Task<bool> Prev() {
            if (!HasSession) return Task.FromResult(false);
            return ControlSession.TrySkipPreviousAsync().AsTask();
        }

        public static Task<bool> Next() {
            if (!HasSession) return Task.FromResult(false);
            return ControlSession.TrySkipNextAsync().AsTask();
        }

        public static void Start() {
            _mediaManager = new MediaManager();

            _mediaManager.OnAnyMediaPropertyChanged += OnAnyMediaPropertyChanged;
            _mediaManager.OnAnyTimelinePropertyChanged += OnAnyTimelinePropertyChanged;
            _mediaManager.OnAnyPlaybackStateChanged += OnAnyPlaybackStateChanged;

            _mediaManager.Start();
            TryUpdateInfo(_mediaManager.GetFocusedSession());
        }

        public static void Stop() {
            _mediaManager.OnAnyPlaybackStateChanged -= OnAnyPlaybackStateChanged;
            _mediaManager.OnAnyTimelinePropertyChanged -= OnAnyTimelinePropertyChanged;
            _mediaManager.OnAnyMediaPropertyChanged -= OnAnyMediaPropertyChanged;

            _mediaManager.Dispose();
            _mediaManager = null;
        }

        private static void OnAnyTimelinePropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties) {
            TryUpdateInfo(mediaSession);
        }

        private static void OnAnyMediaPropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties) {
            TryUpdateInfo(mediaSession);
        }

        private static void OnAnyPlaybackStateChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo) {
            TryUpdateInfo(mediaSession);
        }

        private static bool TryUpdateInfo(MediaManager.MediaSession mediaSession) {
            if (mediaSession == null) return false;
            if (mediaSession.ControlSession == null) return false;

            LastMediaProperties = mediaSession.ControlSession.TryGetMediaPropertiesAsync().AsTask().Result;
            LastTimelineProperties = mediaSession.ControlSession.GetTimelineProperties();
            LastPlaybackInfo = mediaSession.ControlSession.GetPlaybackInfo();

            return true;
        }
    }
}
