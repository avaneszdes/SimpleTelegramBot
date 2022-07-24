using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleTelegramBot.Configuration;
using SimpleTelegramBot.Services;

namespace SimpleTelegramBot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
           await CreateHostBuilder(args);
        }

        private static async Task CreateHostBuilder(string[] args) =>
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<BotOptions>(context.Configuration.GetSection("BotOptions"));
                    services.AddTransient<ITelegramService, TelegramService>();
                    services.AddSingleton<IHostedService, TelegramBot.TelegramBot>();
                    services.AddHostedService<TelegramBot.TelegramBot>();
                })
                .Build()
                .RunAsync();
        // private static async Task Main(string[] args)
        // {
        //     
        //     
        //     
        //     
        //     // await InstagramBase.Initialize();
        //     // Console.WriteLine("Bot started to work " + Bot.GetMeAsync().Result.FirstName);
        //     //
        //     // var cts = new CancellationTokenSource();
        //     // var cancellationToken = cts.Token;
        //     //
        //     // var receiverOptions = new ReceiverOptions
        //     // {
        //     //     AllowedUpdates = { },
        //     // };
        //     //
        //     // Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        //     //
        //     // Console.ReadLine();
        // }
    }
}