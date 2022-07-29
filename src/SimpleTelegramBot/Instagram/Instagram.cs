using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using SimpleTelegramBot.Configuration;

namespace SimpleTelegramBot.Instagram
{
    public class InstagramBase
    {
        private static IInstaApi? _instaApi;

        public static async Task Initialize(InstagramCredentials credentials)
        {
            if (_instaApi == null)
            {
                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(new UserSessionData()
                    {
                        UserName = credentials.Username,
                        Password = credentials.Password
                    })
                    .UseLogger(new DebugLogger(LogLevel.Exceptions))
                    .Build();

                var loginResult = await _instaApi.LoginAsync();
                if (loginResult.Succeeded)
                {
                    Console.WriteLine("Logged In");
                }
            }
        }

        public static async Task<string> GetResourceUrl(string resource)
        {
            dynamic dynamic = await (await _instaApi.HttpRequestProcessor.Client.GetAsync(
                "https://www.instagram.com/p/CFr6G-whXxp/?__a=1&__d=dis")).Content.ReadAsStringAsync();

            dynamic resourceUrl = dynamic.graphql;

            return resourceUrl;
        }
        public static async Task<Stream?> GetMediaById(string resourceId)
        {

            // var a = new InstagramCredentials()
            // {
            //     Password = "qwertasdfgzxcvb94",
            //     Username = "+375293498702"
            // };
            
            // Initialize(a);


            
            var mediaId = await _instaApi
                .MediaProcessor
                .GetMediaIdFromUrlAsync(new Uri(resourceId, UriKind.RelativeOrAbsolute));
            var media = await _instaApi
                .MediaProcessor
                .GetMediaByIdAsync(mediaId.Value);

            var urlToMedia = GetUriForMedia(media.Value.Carousel, media?.Value?.Videos, media?.Value?.Images);

            if (urlToMedia is null)
            {
                return default;
            }

            using var httpClient = new HttpClient();
            return await httpClient.GetStreamAsync(urlToMedia);
        }

        private static Uri? GetUriForMedia(InstaCarousel instaCarousel, IReadOnlyCollection<InstaVideo> instaVideos,
            IReadOnlyCollection<InstaImage> instaImages)
        {
            if (instaCarousel != null && instaCarousel[0].Images != null)
            {
                return new Uri(instaCarousel[0]?.Images?.FirstOrDefault()?.Uri, UriKind.Absolute);
            }

            if (instaCarousel != null && instaCarousel[0].Videos != null)
            {
                return new Uri(instaCarousel[0]?.Videos?.FirstOrDefault()?.Uri, UriKind.Absolute);
            }

            if (instaVideos?.Count > 0)
            {
                return new Uri(instaVideos?.FirstOrDefault()?.Uri, UriKind.Absolute);
            }

            if (instaImages?.Count > 0)
            {
                return new Uri(instaImages?.FirstOrDefault()?.Uri, UriKind.Absolute);
            }

            return default;
        }

        public static Task<IResult<InstaUser>> GetUserDataByUsername(string username) =>
            _instaApi.UserProcessor.GetUserAsync(username);


        public static async Task<IResult<InstaUserShortList>> GetUserAsync(string userName)
        {
             return await _instaApi.UserProcessor.GetUserFollowersAsync(userName, PaginationParameters.MaxPagesToLoad(5));

        }
    }
}