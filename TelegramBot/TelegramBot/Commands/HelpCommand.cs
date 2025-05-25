using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Ninject;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class HelpCommand : ICommand
{
    private IEnumerable<ICommand> Commands { get; }
    private IKeyboards Keyboards { get; }
    
    public string Command => "/start";
    public string Name => "Помощь";
    public string Description => "показать список доступных команд";

    public HelpCommand(IEnumerable<ICommand> commands, IKeyboards keyboards)
    {
        Commands = commands;
        Keyboards = keyboards;
    }
    
    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        var helpMessage = "Добро пожаловать в бота, который отправляет уведомления о новых научных статьях!\n" +
                          $"Ограничение по максимальному числу запросов: {Bot.MaxQueries}.\n" +
                          $"Число подключенных запросов: {user.Queries.Count}.\n" +
                          "В боте доступны следующие команды:\n\n";
        helpMessage = Commands
            .Aggregate(helpMessage, (current, cmd) => current + $"{cmd.Command} - {cmd.Description}\n");

        await botClient.SendMessage(chatId: user.Id, text: helpMessage,
            replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
    }
}