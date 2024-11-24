namespace Bot.TelegramBot.Commands;

public interface ICommandFactory
{
    ICommand CreateCommand(User user, string? message, CancellationToken cancellationToken);
}