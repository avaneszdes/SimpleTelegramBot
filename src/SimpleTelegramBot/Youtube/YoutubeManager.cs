using NAudio.Wave;
using VideoLibrary;
using Youtube;

namespace SimpleTelegramBot.Youtube
{
    public class YoutubeManager
    {
        private const long VideoBytesLimit = 50_000_000;
        private static readonly YouTube YouTube = new();

        public static async Task<YoutubeManagerAudio?> ExtractAudioStreamAsync(string resource)
        {
            var video = (await YouTube.GetAllVideosAsync(resource)).ToArray();

            if (!video.Any())
            {
                return null;
            }

            var videoWithMaxBitrate = video.Where(x => x.AudioBitrate > 0).MinBy(x => x.AudioBitrate);
            var videoBytes = await videoWithMaxBitrate.GetBytesAsync();
            var outStream = new MemoryStream();

            await using var mediaReader = new StreamMediaFoundationReader(new MemoryStream(videoBytes));
            {
                if (mediaReader.CanRead)
                {
                    var a = new WaveFileWriter(outStream, new Mp3WaveFormat(128, Int32.MaxValue, 4, 128));
                    a.Write(videoBytes);
                    // WaveFileWriter.WriteWavFileToStream(outStream, mediaReader);
                    outStream.Seek(0, SeekOrigin.Begin);
                    return new YoutubeManagerAudio(videoWithMaxBitrate.Title, outStream);
                }
            }

            return null;
        }

        public static async Task<YoutubeManagerVideo?> GetVideoStreamAsync(string resource)
        {
            var videos = (await YouTube.GetAllVideosAsync(resource)).ToArray();

            if (!videos.Any())
            {
                return null;
            }

            var youTubeVideo = videos
                .OrderByDescending(e => e.AudioBitrate)
                .ThenByDescending(x => x.Resolution)
                .First();
            
            var escapedTitle = youTubeVideo?.Title
                .Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("`", "\\`")
                .Replace("!", "\\!")
                .Replace(".", "\\.")
                .Replace("]", "\\]");
            
            if (youTubeVideo?.ContentLength > VideoBytesLimit)
            {
                return new YoutubeManagerVideo(youTubeVideo.Uri, title: escapedTitle);
            }

            var videoStream = await youTubeVideo?.StreamAsync();

            return new YoutubeManagerVideo(video: videoStream, title: escapedTitle);
        }
    }
}