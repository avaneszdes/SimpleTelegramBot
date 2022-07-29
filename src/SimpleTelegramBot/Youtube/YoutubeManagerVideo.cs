namespace SimpleTelegramBot.Youtube
{
    public record YoutubeManagerVideo
    {
        public YoutubeManagerVideo(string uri = null, Stream video = null, string title = null)
        {
            Uri = uri;
            Video = video;
            Title = title;
        }

        public string? Uri { get; }
        public Stream? Video { get; }
        public string? Title { get; }
    }
}