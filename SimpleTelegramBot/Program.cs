using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Instagram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using VideoLibrary;
using Telegram.Bot.Extensions.Polling;
using File = System.IO.File;

namespace SimpleTelegramBot
{
    internal class Program
    {
        private const int VideoBytesLimit = 50000 * 1000;
        private static ITelegramBotClient bot = new TelegramBotClient("1105618244:AAHLmQmptyjG7_LYA8Ue4Nlxy4_Zt5Y0fJE");
        private static YouTube youTube = YouTube.Default;

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));


            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;

                try
                {
                    if (message.Text.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Hi!", cancellationToken: cancellationToken);
                        return;
                    }

                    if (message.Text.ToLower().StartsWith("https://youtu"))
                    {
                        try
                        {
                            var video = await youTube.GetVideoAsync(message.Text);
                            var bytes = await video.GetBytesAsync();

                            if (bytes.Length > VideoBytesLimit)
                            {
                                await botClient.SendTextMessageAsync(message.Chat,
                                    "Oops), at the moment we are not able to process files more than 50 Mb",
                                    cancellationToken: cancellationToken);
                                return;
                            }

                            await using (var stream = new MemoryStream(bytes))
                            {
                                await botClient.SendVideoAsync(
                                    message.Chat,
                                    new InputOnlineFile(stream),
                                    video.Info.LengthSeconds,
                                    cancellationToken: cancellationToken);
                            }

                            return;
                        }
                        catch (Exception e)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Oops), something went wrong",
                                cancellationToken: cancellationToken);
                        }
                    }

                    if (message.Text.ToLower().StartsWith("https://www.instagram.com"))
                    {
                        var res = await InstagramBase.GetMediaById(message.Text);
                        if (res is null)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Oops), something went wrong",
                                cancellationToken: cancellationToken);
                        }

                        await botClient.SendVideoAsync(
                            message.Chat,
                            new InputOnlineFile(res),
                            cancellationToken: cancellationToken);


                        return;
                    }

                    if (message.Text.ToLower().StartsWith("userdata:"))
                    {
                        // var a = await InstagramBase.GetMediaByResource(message.Text.Split(':')[1]);
                        //
                        // await botClient.SendVideoAsync(
                        //     message.Chat,
                        //     new InputOnlineFile(a),
                        //     cancellationToken: cancellationToken);


                        
                        var a = await InstagramBase.GetUserAsync(message.Text.Split(':')[1]);

                        foreach (var d in a.Value)
                        {
                            await botClient.SendPhotoAsync(message.Chat, d.ProfilePicture, cancellationToken: cancellationToken);
                            
                            await botClient.SendTextMessageAsync(message.Chat, d.UserName, cancellationToken: cancellationToken);
                        }
                        
                        
                        
                        var res = (await InstagramBase.GetUserDataByUsername(message.Text.Split(':')[1])).Value;
                        if (res is null)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Oops), something went wrong",
                                cancellationToken: cancellationToken);
                        }

                        
                       
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{res?.FullName}\r\n-{res?.FollowersCount}\r\n-{res.FriendshipStatus.Following}",
                            cancellationToken: cancellationToken);


                        return;
                    }
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Oops), something went wrong",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        static async Task Main(string[] args)
        {
            await InstagramBase.Initialize();
            // SaveVideoFromInsta("124");
            Console.WriteLine("Bot started to work " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            Console.ReadLine();
            // SaveVideoToDisk("https://youtu.be/XsSZ_GJsa64");
        }

        static void SaveVideoToDisk(string link)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(link);

            File.WriteAllBytes(@"D:\" + video.FullName, video.GetBytes());
        }


        static void SaveVideoFromInsta(string url)
        {
            // WebClient client = new WebClient();
            // client.DownloadFile(
            //     "https://instagram.fmsq1-1.fna.fbcdn.net/v/t50.2886-16/290805234_733464001236578_5301085803044974116_n.mp4?efg=eyJ2ZW5jb2RlX3RhZyI6InZ0c192b2RfdXJsZ2VuLjcyMC5jbGlwcy5iYXNlbGluZSIsInFlX2dyb3VwcyI6IltcImlnX3dlYl9kZWxpdmVyeV92dHNfb3RmXCJdIn0&_nc_ht=instagram.fmsq1-1.fna.fbcdn.net&_nc_cat=102&_nc_ohc=va-E893kecYAX94V3XV&edm=ALQROFkBAAAA&vs=769082887602227_293746894&_nc_vs=HBksFQAYJEdQSlZWUkZpemotbEZKc0NBQ1N1WEpaelBaRkpicV9FQUFBRhUAAsgBABUAGCRHTlJsWnhIVVZSWXBQUHNCQUhPRGgwX0JQbVV1YnFfRUFBQUYVAgLIAQAoABgAGwAVAAAmxJSTpr3Vuj8VAigCQzMsF0BObtkWhysCGBJkYXNoX2Jhc2VsaW5lXzFfdjERAHX%2BBwA%3D&_nc_rid=1d2272192a&ccb=7-5&oe=62CC8CFA&oh=00_AT8Fc07U-zQJdx0Oe6l7s2qKzaKS3q9TpW-AiMoyFqZ7Ww&_nc_sid=30a2ef",
            //     @"D:\" + "3333");

            var httpClient = new HttpClient();
            httpClient.GetStringAsync(new Uri("https://www.instagram.com/reel/Cfq8Bq8Kuvf/?igshid=YmMyMTA2M2Y=",
                UriKind.Absolute));
        }
    }
}