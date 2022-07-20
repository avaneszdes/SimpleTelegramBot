using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Entities;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace SimpleTelegramBot
{
    public class InstaScraper
    {
        public static async Task<InstagramUser> ScrapeInstagram(string profile)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://www.instagram.com/vlad_avanesov/");
                if (response.IsSuccessStatusCode)
                {
                    // create html document
                    var htmlBody = await response.Content.ReadAsStringAsync();
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlBody);

                    // select script tags
                    var scripts = htmlDocument.DocumentNode.SelectNodes("/html/body/script");

                    // preprocess result
                    var uselessString = "window._sharedData = ";
                    var scriptInnerText = scripts[0].InnerText
                        .Substring(uselessString.Length)
                        .Replace(";", "");

                    // serialize objects and fetch the user data
                    dynamic jsonStuff = JObject.Parse(scriptInnerText);
                    dynamic userProfile = jsonStuff["entry_data"]["ProfilePage"][0]["graphql"]["user"];

                    // create an InstagramUser
                    var instagramUser = new InstagramUser
                    {
                        FullName = userProfile.full_name,
                        FollowerCount = userProfile.edge_followed_by.count,
                        FollowingCount = userProfile.edge_follow.count
                    };
                    return instagramUser;
                } else
                {
                    throw new Exception($"Something wrong happened {response.StatusCode} - {response.ReasonPhrase} - {response.RequestMessage}");
                }
            }
        }

        public static async Task FileDownload(string url)
        {
            //?__a=1
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://www.instagram.com/p/Cfq8Bq8Kuvf/?a=1");
                if (response.IsSuccessStatusCode)
                {
                    var res =  await response.Content.ReadAsStringAsync();
                    // dynamic jsonResponse = JObject.Parse(res.);
                    // var videoUrl = jsonResponse.graphql.video_url;
                    //
                    // var webClient = new WebClient();
                    //
                    // webClient.DownloadFileAsync(videoUrl, @"D:\" + "444");
                }
            }
        }
    }
}