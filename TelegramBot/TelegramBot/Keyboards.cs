using Bot.TelegramBot.Commands;
using Ninject;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Bot;

public static class Keyboards
{
    public static ReplyKeyboardMarkup CommandsKeyboard
    {
        get
        {
            var commands = KernelHandler.Kernel.GetAll<ICommand>();
            var buttons = commands.Select(cmd => new KeyboardButton(cmd.Name)).ToArray();

            return new ReplyKeyboardMarkup(buttons)
            {
                OneTimeKeyboard = true,
                ResizeKeyboard = true
            };
        }
    }
    
    public static ReplyKeyboardMarkup ConfirmationKeyboard => new([
        [
            new KeyboardButton("Да"),
            new KeyboardButton("Нет")
        ]
    ])
    {
        OneTimeKeyboard = true,
        ResizeKeyboard = true
    };
}