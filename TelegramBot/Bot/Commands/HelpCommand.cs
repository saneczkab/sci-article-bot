using Bot.Bot;
using Ninject;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class HelpCommand : ICommand
{
    public string Command => "/help";
    public string Description => "показать список доступных команд";

    public async Task Execute(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken, string message)
    {
        var helpMessage = "Добро пожаловать в бота, который отправляет уведомления о новых научных статьях!\n" +
                          "В боте доступны следующие команды:\n";
        var commandMetadata = KernelHandler.Kernel.GetAll<ICommand>();
        
        foreach (var cmd in commandMetadata)
        {
            helpMessage += $"{cmd.Command} - {cmd.Description}\n";
        }

        await botClient.SendMessage(
            chatId: user.Id,
            text: helpMessage,
            replyMarkup: MessageHandler.CommandsKeyboard,
            cancellationToken: cancellationToken
        );
    }
}