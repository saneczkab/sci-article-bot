using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class UnknownCommand : ICommand
{
    private IKeyboards Keyboards { get; }
    public string Command => "";
    public string Name => "";
    public string Description => "";

    public UnknownCommand(IKeyboards keyboards)
    {
        Keyboards = keyboards;
    }
    
    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        await botClient.SendMessage(chatId: user.Id,
            text: "Неизвестная команда. Для получения списка команд введите /help",
            cancellationToken: cancellationToken, replyMarkup: Keyboards.CommandsKeyboard);
    }
}