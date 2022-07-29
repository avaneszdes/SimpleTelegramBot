using Microsoft.Extensions.Options;
using SimpleTelegramBot.Configuration;
using SimpleTelegramBot.Instagram;
using SimpleTelegramBot.Youtube;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SimpleTelegramBot.Services
{
    public class TelegramService: ITelegramService
    {
        private readonly BotOptions _options;
        
        public TelegramService(IOptions<BotOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(typeof(BotOptions).ToString(), "Value can not be null");
        }
        
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message?.Text;
                    var chatId = update.Message?.Chat;
                    try
                    {
                        switch (message)
                        {
                            case "/start":
                            {
                                await botClient.SendStickerAsync(chatId,
                                    "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-fred.webp",
                                    cancellationToken: cancellationToken);
                                await botClient.SendTextMessageAsync(chatId, "Hi!",
                                    cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when message.StartsWith("https://you"):
                            {
                                var video = await YoutubeManager.GetVideoStreamAsync(message);
                                if (video?.Video == null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, video.Title, ParseMode.MarkdownV2,
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
                                                CallbackData = message
                                            }
                                        }), cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendVideoAsync(chatId, video.Video,
                                        replyMarkup: new InlineKeyboardMarkup(new[]
                                        {
                                            new InlineKeyboardButton("2")
                                            {
                                                Text = "Get audio from video",
                                                CallbackData = message
                                            }
                                        }), cancellationToken: cancellationToken);
                                }

                                break;
                            }
                            case { } when message.StartsWith("https://www.instagram.com"):
                            {
                                var mediaStream = await InstagramBase.GetMediaById(message);
                                if (mediaStream is null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Oops), something went wrong",
                                        cancellationToken: cancellationToken);
                                }

                                await botClient.SendVideoAsync(chatId, new InputOnlineFile(mediaStream),
                                    cancellationToken: cancellationToken);
                                break;
                            }
                            case { } when message.StartsWith("userdata:"):
                            {
                                var a = await InstagramBase.GetUserAsync(message.Split(':')[1]);

                                foreach (var d in a.Value)
                                {
                                    await botClient.SendPhotoAsync(chatId, d.ProfilePicture,
                                        cancellationToken: cancellationToken);

                                    await botClient.SendTextMessageAsync(chatId, d.UserName,
                                        cancellationToken: cancellationToken);
                                }

                                var res = (await InstagramBase.GetUserDataByUsername(message.Split(':')[1])).Value;
                                if (res is null)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Oops), something went wrong",
                                        cancellationToken: cancellationToken);
                                }


                                await botClient.SendTextMessageAsync(chatId,
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
                                await botClient.SendAudioAsync(chat, new InputOnlineFile(audio.Audio, audio.Title + ".mp3" ), title: audio.Title,
                                    cancellationToken: cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                await botClient.SendTextMessageAsync(chat, ex.Message,
                                    cancellationToken: cancellationToken);
                            }

                            break;
                        }
                    }

                    break;
            }
        }
    }
}