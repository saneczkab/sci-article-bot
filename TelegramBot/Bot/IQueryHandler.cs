using Telegram.Bot;

namespace Bot.TelegramBot;

public interface IQueryHandler
{
    public static abstract Task Handle(ITelegramBotClient botClient, User user, CancellationToken cancellationToken);
    
    public static abstract Task GetText(ITelegramBotClient botClient, User user, string message, 
        CancellationToken cancellationToken);
    
    public static abstract Task GetConfirmation(ITelegramBotClient botClient, User user, string message, 
        CancellationToken cancellationToken);
}