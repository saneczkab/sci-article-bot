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

        articleProcessor.ScheduleDailyTask(new TimeSpan(8, 17, 0)); // UTC
        _ = Task.Run(ArticleProcessor.BlockingRun, cts.Token);

        _ = Scheduler.RunWithInterval(
            TimeSpan.FromSeconds(1),
            () => messageHandler.GetNewArticles(_botClient, cts.Token));

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