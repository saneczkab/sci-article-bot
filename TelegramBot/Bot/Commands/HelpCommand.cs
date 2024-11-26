using Bot.Bot;
using Ninject;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class HelpCommand : ICommand
{
    public string Command => "/help";
    public string Name => "Помощь";
    public string Description => "показать список доступных команд";

    public async Task Execute(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken, string message)
    {
        var helpMessage = "Добро пожаловать в бота, который отправляет уведомления о новых научных статьях!\n" +
                          $"Ограничение по максимальному числу запросов: {Bot.MaxQueries}.\n" +
                          $"Число подключенных запросов: {user.Queries.Count}.\n" +
                          "В боте доступны следующие команды:\n\n";
        var commands = KernelHandler.Kernel.GetAll<ICommand>();
        helpMessage = commands
            .Aggregate(helpMessage, (current, cmd) => current + $"{cmd.Command} - {cmd.Description}\n");

        await botClient.SendMessage(chatId: user.Id, text: helpMessage, 
            replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
    }
}