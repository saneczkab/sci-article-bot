using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Bot.TelegramBot;

public static class Bot
{
    private const string Token = "7377758210:AAEnYbnKCdZ5CNXzSxsN0XRl6iPZLs8BOdA";
    private static TelegramBotClient? _botClient;
    public const int MaxQueries = 4;

    private static async Task Main()
    {
        _botClient = new TelegramBotClient(Token);
        var cts = new CancellationTokenSource();
        await _botClient.GetMe(cancellationToken: cts.Token);

        _botClient.StartReceiving(
            MessageHandler.HandleUpdate,
            MessageHandler.HandleError,
            new ReceiverOptions(),
            cts.Token);

        _ = Scheduler.RunWithInterval(
            TimeSpan.FromSeconds(1),
            () => MessageHandler.GetNewArticles(_botClient, cts.Token));
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