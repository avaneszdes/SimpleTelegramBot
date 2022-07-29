using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SimpleTelegramBot.Configuration;
using SimpleTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using InstagramBase = SimpleTelegramBot.Instagram.InstagramBase;

namespace SimpleTelegramBot.TelegramBot
{
    public class TelegramBot : IHostedService
    {
        private static ITelegramService? _telegramService;

        private readonly ITelegramBotClient _bot;        

        public TelegramBot(IOptions<BotOptions> options, ITelegramService telegramService)
        {
            _telegramService = telegramService;
            InstagramBase.Initialize(options.Value.InstagramCredentials).GetAwaiter().GetResult();
            _bot = new TelegramBotClient(options.Value.BotToken);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var me = await _bot.GetMeAsync(cancellationToken);
                Console.WriteLine("Bot started to work " + me.FirstName);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { },
                };

                _bot.StartReceiving(_telegramService.HandleUpdateAsync, 
                    _telegramService.HandleErrorAsync, receiverOptions, cancellationToken);

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}