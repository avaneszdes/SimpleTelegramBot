using System.IO;

namespace Youtube
{
    public record YoutubeManagerVideo
    {
        public YoutubeManagerVideo(string uri = null, Stream video = null)
        {
            Uri = uri;
            Video = video;
        }

        public string? Uri { get; }
        public Stream? Video { get; }
    }
}