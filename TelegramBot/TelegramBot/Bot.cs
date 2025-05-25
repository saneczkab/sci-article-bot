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
        var articleProcessor = kernel.Get<ArticleProcessor>();

        _botClient.StartReceiving(
            messageHandler.HandleUpdate,
            messageHandler.HandleError,
            new ReceiverOptions(),
            cts.Token);

        var scanTime = new TimeSpan(3, 0, 0); // UTC
        articleProcessor.ScheduleDailyTask(scanTime, _botClient, messageHandler);
        _ = Task.Run(ArticleProcessor.BlockingRun, cts.Token);

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        finally
        {
            await cts.CancelAsync();
        }
    }
}