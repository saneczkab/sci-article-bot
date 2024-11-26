using Bot.Models;
using Telegram.Bot;

namespace Bot.TelegramBot.Interfaces;

public interface ICommand
{
    string Command { get; }
    string Name { get; }
    string Description { get; }

    Task Execute(ITelegramBotClient botClient, User user, CancellationToken cancellationToken, string message);
}