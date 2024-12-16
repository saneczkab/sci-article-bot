using Bot.Models;

namespace Bot.TelegramBot.Interfaces;

public interface ICommandFactory
{
    ICommand? GetCommand(User user, string? message, CancellationToken cancellationToken);
}