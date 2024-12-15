using Bot.Models;

namespace Bot.TelegramBot.Interfaces;

public interface ICommandFactory
{
    ICommand? CreateCommand(User user, string? message, CancellationToken cancellationToken);
}