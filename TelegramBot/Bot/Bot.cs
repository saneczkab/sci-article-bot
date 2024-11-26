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
      
        Scheduler.RunDailyTask(10, 00, async () =>
        {
            await MessageHandler.GetNewArticles(_botClient, cts.Token);
        });
        
        Console.ReadKey();
        await cts.CancelAsync();
    }
}