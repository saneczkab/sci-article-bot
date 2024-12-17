using Ninject;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Bot.TelegramBot;

public static class Bot
{
    private static readonly string Token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
    private static TelegramBotClient? _botClient;
    public const int MaxQueries = 4;

    private static async Task Main()
    {
        _botClient = new TelegramBotClient(Token);
        var cts = new CancellationTokenSource();
        await _botClient.GetMe(cancellationToken: cts.Token);
        
        var kernel = new StandardKernel(new BotModule());
        var messageHandler = kernel.Get<MessageHandler>();

        _botClient.StartReceiving(
            messageHandler.HandleUpdate,
            messageHandler.HandleError,
            new ReceiverOptions(),
            cts.Token);

        _ = Scheduler.RunWithInterval(
            TimeSpan.FromSeconds(1),
            () => messageHandler.GetNewArticles(_botClient, cts.Token));
        try
        {
            Console.WriteLine("Нажмите на любую клавишу, чтобы остановить программу:");
            Console.ReadKey(true);
        }
        finally
        {
            await cts.CancelAsync();
        }
    }
}