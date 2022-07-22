using System.IO;

namespace Youtube
{
    public record YoutubeManagerVideo
    {
        public YoutubeManagerVideo(string uri) => Uri = uri;
        public YoutubeManagerVideo(Stream video) => Video = video;
        public string Uri { get; } = null!;
        public Stream Video { get; } = null!;
    }
}