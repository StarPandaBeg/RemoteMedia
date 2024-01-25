using RemoteMedia.Application.Util;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace RemoteMedia.Application.Commands {
    class InfoCommand : ICommand {

        public Task<byte[]> Execute() {
            var data = new MediaInfo();

            var mediaProperties = MediaManagerHelper.LastMediaProperties;
            var playbackInfo = MediaManagerHelper.LastPlaybackInfo;
            var timelineProperties = MediaManagerHelper.LastTimelineProperties;

            data.Title = mediaProperties?.Title ?? data.Title;
            data.Author = mediaProperties?.Artist ?? data.Author;
            data.IsPlaying = playbackInfo != null
                && playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            data.Position = timelineProperties?.Position.TotalSeconds ?? 0;
            data.Duration = timelineProperties?.EndTime.TotalSeconds ?? 0;
            data.HasNext = playbackInfo?.Controls.IsNextEnabled ?? false;
            data.HasPrev = playbackInfo?.Controls.IsPreviousEnabled ?? false;

            data.Volume = SystemAudio.GetVolume();

            var jsonData = JsonSerializer.SerializeToUtf8Bytes(data);
            var response = jsonData.Prepend((byte)CommandStatus.OK).ToArray();

            return Task.FromResult(response);
        }

        private class MediaInfo {
            [JsonPropertyName("is_playing")]
            public bool IsPlaying { get; set; } = false;

            [JsonPropertyName("song_title")]
            public string Title { get; set; } = "No Title";

            [JsonPropertyName("song_author")]
            public string Author { get; set; } = "Unknown";

            [JsonPropertyName("song_position")]
            public double Position { get; set; } = 0;

            [JsonPropertyName("song_duration")]
            public double Duration { get; set; } = 0;

            [JsonPropertyName("volume")]
            public int Volume { get; set; } = 0;

            [JsonPropertyName("has_next")]
            public bool HasNext { get; set; } = false;

            [JsonPropertyName("has_prev")]
            public bool HasPrev { get; set; } = false;
        }
    }
}
