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
        private const int VideoBytesLimit = 50_000_000;
        private static readonly ITelegramBotClient Bot = new TelegramBotClient("1105618244:AAHLmQmptyjG7_LYA8Ue4Nlxy4_Zt5Y0fJE");
        private static readonly YouTube YouTube = YouTube.Default;

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    var chatId = message?.Chat;
                    var messageText = message?.Text?.ToLower();
                    try
                    {
                        switch (messageText)
                        {
                            case "/start":
                            {
                                await botClient.SendStickerAsync(
                                     chatId,
                                     "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-fred.webp",
                                    cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(chatId, "Hi!", cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when messageText.StartsWith("https"):
                            {
                                var video = await YouTube.GetVideoAsync(messageText);
                                var bytes = await video.GetUriAsync();

                                if (bytes.Length > VideoBytesLimit)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Oops), at the moment we are not able to process files more than 50 Mb", cancellationToken: cancellationToken);
                                    return;
                                }

                                await botClient.SendVideoAsync(chatId, bytes, cancellationToken: cancellationToken);
                                // await using (var stream = new MemoryStream(bytes))
                                // {
                                //     
                                // }
                                break;
                            }
                            case { } when messageText.StartsWith("https://www.instagram.com"):
                            {
                                var res = await InstagramBase.GetMediaById(messageText);
                                if (res is null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Oops), something went wrong",
                                        cancellationToken: cancellationToken);
                                }

                                await botClient.SendVideoAsync(chatId, new InputOnlineFile(res), cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when messageText.StartsWith("userdata:"):
                            {
                                var a = await InstagramBase.GetUserAsync(message.Text.Split(':')[1]);

                                foreach (var d in a.Value)
                                {
                                    await botClient.SendPhotoAsync(message.Chat, d.ProfilePicture,
                                        cancellationToken: cancellationToken);

                                    await botClient.SendTextMessageAsync(message.Chat, d.UserName,
                                        cancellationToken: cancellationToken);
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


                                break;
                            }
                        }
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Oops), something went wrong",
                            cancellationToken: cancellationToken);
                    }
                }
                    break;
                case UpdateType.Unknown:
                    break;
                case UpdateType.InlineQuery:
                    break;
                case UpdateType.ChosenInlineResult:
                    break;
                case UpdateType.CallbackQuery:
                    break;
                case UpdateType.EditedMessage:
                    break;
                case UpdateType.ChannelPost:
                    break;
                case UpdateType.EditedChannelPost:
                    break;
                case UpdateType.ShippingQuery:
                    break;
                case UpdateType.PreCheckoutQuery:
                    break;
                case UpdateType.Poll:
                    break;
                case UpdateType.PollAnswer:
                    break;
                case UpdateType.MyChatMember:
                    break;
                case UpdateType.ChatMember:
                    break;
                case UpdateType.ChatJoinRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private static async Task Main(string[] args)
        {
            await InstagramBase.Initialize();
            Console.WriteLine("Bot started to work " + Bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            Console.ReadLine();
        }

        static void SaveVideoToDisk(string link)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(link);

            File.WriteAllBytes(@"D:\" + video.FullName, video.GetBytes());
        }
    }
}