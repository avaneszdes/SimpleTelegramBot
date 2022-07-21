using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Instagram;
using NAudio.Wave;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using VideoLibrary;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace SimpleTelegramBot
{
    internal class Program
    {
        private const int VideoBytesLimit = 50_000_000;

        private static string _tempUrl = string.Empty;

        private static readonly ITelegramBotClient Bot =
            new TelegramBotClient("1105618244:AAHLmQmptyjG7_LYA8Ue4Nlxy4_Zt5Y0fJE");

        private static readonly YouTube YouTube = YouTube.Default;

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
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
                                await botClient.SendTextMessageAsync(chatId, "Hi!",
                                    cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when messageText.StartsWith("https"):
                            {
                                var video = await YouTube.GetVideoAsync(message.Text);
                                var videoBytes = (await video?.GetBytesAsync()!).Length;
                                var isUnderBytesLimit = videoBytes > VideoBytesLimit;
                                _tempUrl = messageText;
                                if (isUnderBytesLimit)
                                {
                                    await botClient.SendTextMessageAsync(
                                        message.Chat,
                                        "",
                                        parseMode: ParseMode.MarkdownV2,
                                        disableWebPagePreview: true,
                                        replyMarkup: new InlineKeyboardMarkup(new[]
                                        {
                                            new InlineKeyboardButton("1")
                                            {
                                                Text = "Press to download video",
                                                Url = messageText
                                            },
                                            new InlineKeyboardButton("2")
                                            {
                                                Text = "Get audio from video",
                                                CallbackData = message.Text
                                            }
                                        }),
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendVideoAsync(
                                        message.Chat,
                                        await video.StreamAsync(),
                                        replyMarkup: new InlineKeyboardMarkup(new[]
                                        {
                                            new InlineKeyboardButton("2")
                                            {
                                                Text = "Get audio from video",
                                                CallbackData =  message.Text
                                            }
                                        }),
                                        cancellationToken: cancellationToken);
                                }

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

                                await botClient.SendVideoAsync(chatId, new InputOnlineFile(res),
                                    cancellationToken: cancellationToken);
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
                    catch (Exception ex)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, ex.Message,
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

                    var query = update.CallbackQuery?.Data;
                    switch (query)
                    {
                        case {Length: > 0}:
                        {
                            try
                            {
                                var video = await YouTube.GetVideoAsync(update.CallbackQuery.Data);
                                await using (var contentAsMemoryStream = new MemoryStream(await video.GetBytesAsync()))
                                {
                                    await using (var outStream = new MemoryStream())
                                    {
                                        await using (var mediaReader =
                                            new StreamMediaFoundationReader(contentAsMemoryStream))
                                        {
                                            if (mediaReader.CanRead)
                                            {
                                                mediaReader.Seek(0, SeekOrigin.Begin);

                                                WaveFileWriter.WriteWavFileToStream(outStream, mediaReader);
                                                outStream.Seek(0, SeekOrigin.Begin);

                                                await botClient.SendAudioAsync(update.CallbackQuery?.Message?.Chat,
                                                    outStream, title: video.Title,
                                                    cancellationToken: cancellationToken);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendTextMessageAsync(update.CallbackQuery?.Message?.Chat, ex.Message,
                                    cancellationToken: cancellationToken);
                            }
                           

                            break;
                        }
                    }

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