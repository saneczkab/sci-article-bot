using Bot.Bot;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class UnknownCommand : ICommand
{
    public string Command => "";
    public string Name => "";
    public string Description => "";
    
    public async Task Execute(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken, string message)
    {
        await botClient.SendMessage(chatId: user.Id, 
            text: "Неизвестная команда. Для получения списка команд введите /help", 
            cancellationToken: cancellationToken, replyMarkup: Keyboards.CommandsKeyboard);
    }
}