using System.IO;

namespace Youtube
{
    public class YoutubeManagerAudio
    {
        public YoutubeManagerAudio(string title, Stream audio)
        {
            Title = title;
            Audio = audio;
        }

        public string Title { get;}
        public Stream Audio { get;}
    }
}