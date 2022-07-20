using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;

namespace Instagram
{
    public class InstagramBase
    {
        private static IInstaApi InstaApi;

        public static async Task Initialize()
        {
            if (InstaApi == null)
            {
                InstaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(new UserSessionData()
                    {
                        UserName = "+375293498702 ",
                        Password = "qwertasdfgzxcvb94"
                    })
                    .UseHttpClient(new HttpClient()
                    {
                        BaseAddress = new Uri("https://www.instagram.com", UriKind.Absolute)
                    })
                    .UseLogger(new DebugLogger(LogLevel.Exceptions))
                    .Build();

                var loginResult = await InstaApi.LoginAsync();
                if (loginResult.Succeeded)
                {
                    Console.WriteLine("Logged In");
                }
            }
        }

        public static async Task<Stream?> GetMediaById(string resourceId)
        {
            var mediaId = await InstaApi
                .MediaProcessor
                .GetMediaIdFromUrlAsync(new Uri(resourceId, UriKind.RelativeOrAbsolute));
            var media = await InstaApi
                .MediaProcessor
                .GetMediaByIdAsync(mediaId.Value);


            var urlToMedia = GetUriForMedia(media.Value.Carousel, media?.Value?.Videos, media?.Value?.Images);

            if (urlToMedia is null)
            {
                return default;
            }

            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetStreamAsync(urlToMedia);
            }
        }

        private static Uri? GetUriForMedia(InstaCarousel instaCarousel, List<InstaVideo> instaVideos,
            List<InstaImage> instaImages)
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
            InstaApi.UserProcessor.GetUserAsync(username);


        public static async Task<IResult<InstaUserShortList>> GetUserAsync(string userName)
        {
             return await InstaApi.UserProcessor.GetUserFollowersAsync("magnus_wealthy",
                PaginationParameters.MaxPagesToLoad(5));


            var following = await InstaApi.UserProcessor.GetUserFollowingAsync("magnus_wealthy",
                PaginationParameters.MaxPagesToLoad(5));

            
        }
        // public static async Task<Stream> GetMediaByResource(string resource)
        // {
        //     var rr = await InstaApi.WebProcessor;
        //
        //     var aaa = rr.Content.ReadAsStringAsync();
        //     
        //     return rr.Content.ReadAsStream();
        // }
    }
}