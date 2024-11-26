using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public interface ICommand
{
    string Command { get; }
    string Description { get; }
    
    Task Execute(ITelegramBotClient botClient, User user, CancellationToken cancellationToken, string message);
}