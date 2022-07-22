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
using Youtube;

namespace SimpleTelegramBot
{
    internal class Program
    {
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
                    var messageLowerText = message?.Text?.ToLower();
                    try
                    {
                        switch (messageLowerText)
                        {
                            case "/start":
                            {
                                await botClient.SendStickerAsync(chatId,
                                    "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-fred.webp",
                                    cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(chatId, "Hi!", cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when messageLowerText.StartsWith("https"):
                            {
                                var video = await YoutubeManager.GetVideoStreamAsync(message.Text);
                                if (video?.Video == null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "", ParseMode.MarkdownV2,
                                        disableWebPagePreview: true, replyMarkup: new InlineKeyboardMarkup(new[]
                                        {
                                            new InlineKeyboardButton("1")
                                            {
                                                Text = "Press to download video",
                                                Url = video?.Uri
                                            },
                                            new InlineKeyboardButton("2")
                                            {
                                                Text = "Get audio from video",
                                                CallbackData = message.Text
                                            }
                                        }), cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendVideoAsync(message.Chat, video.Video, replyMarkup: new InlineKeyboardMarkup(new[]
                                        {
                                            new InlineKeyboardButton("2")
                                            {
                                                Text = "Get audio from video",
                                                CallbackData = message.Text
                                            }
                                        }), cancellationToken: cancellationToken);
                                }

                                break;
                            }
                            case { } when messageLowerText.StartsWith("https://www.instagram.com"):
                            {
                                var mediaStream = await InstagramBase.GetMediaById(messageLowerText);
                                if (mediaStream is null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Oops), something went wrong", cancellationToken: cancellationToken);
                                }

                                await botClient.SendVideoAsync(chatId, new InputOnlineFile(mediaStream), cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when messageLowerText.StartsWith("userdata:"):
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
                        await botClient.SendTextMessageAsync(chatId, ex.Message, cancellationToken: cancellationToken);
                    }

                    break;
                }
                case UpdateType.CallbackQuery:

                    var query = update.CallbackQuery?.Data;
                    var chat = update.CallbackQuery?.Message?.Chat;
                    switch (query)
                    {
                        case {Length: > 0}:
                        {
                            try
                            {
                                var audio = await YoutubeManager.ExtractAudioStreamAsync(query);
                                await botClient.SendAudioAsync(chat, audio.Audio, title: audio.Title, cancellationToken: cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendTextMessageAsync(chat, ex.Message, cancellationToken: cancellationToken);
                            }

                            break;
                        }
                    }

                    break;
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
                AllowedUpdates = { },
            };

            Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            Console.ReadLine();
        }
    }
}