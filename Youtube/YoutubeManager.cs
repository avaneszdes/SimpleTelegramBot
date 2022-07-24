using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;
using VideoLibrary;

namespace Youtube
{
    public class YoutubeManager
    {
        private const long VideoBytesLimit = 50_000_000;
        private static readonly YouTube YouTube = new ();

        public static async Task<YoutubeManagerAudio?> ExtractAudioStreamAsync(string resource)
        {
            var video = (await YouTube.GetAllVideosAsync(resource)).ToArray();
            
            if (!video.Any())
            {
                return null;
            }
            
            var videoWithMaxBitrate = video.MaxBy(x => x.AudioBitrate);
            var videoBytes = await videoWithMaxBitrate.GetBytesAsync();
            var outStream = new MemoryStream();
            
            await using var mediaReader = new StreamMediaFoundationReader(new MemoryStream(videoBytes));
            {
                if (mediaReader.CanRead)
                {
                    WaveFileWriter.WriteWavFileToStream(outStream, mediaReader);
                    outStream.Seek(0, SeekOrigin.Begin);
                    return new YoutubeManagerAudio(videoWithMaxBitrate.Title, outStream);
                } 
            }
           
            return null;
        }
        
        public static async Task<YoutubeManagerVideo?> GetVideoStreamAsync(string resource)
        {
            var video = (await YouTube.GetAllVideosAsync(resource)).ToArray();
            
            if (!video.Any())
            {
                return null;
            }
            var videoWithMaxResolution = video.MaxBy(x => x.Resolution);

            if (videoWithMaxResolution.ContentLength > VideoBytesLimit)
            {
                return new YoutubeManagerVideo(videoWithMaxResolution.Uri);
            }
            var videoStream = await videoWithMaxResolution.StreamAsync();

            return new YoutubeManagerVideo(video: videoStream);
        }
    }   
}

