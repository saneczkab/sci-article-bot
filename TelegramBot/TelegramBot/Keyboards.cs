using Bot.TelegramBot.Interfaces;
using Ninject;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot;

public class Keyboards : IKeyboards
{
    private static IEnumerable<ICommand> _commands;
    
    public Keyboards(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }
    
    public ReplyKeyboardMarkup CommandsKeyboard
    {
        get
        {
            var buttons = _commands.Select(cmd => new KeyboardButton(cmd.Name)).ToArray();

            return new ReplyKeyboardMarkup(buttons)
            {
                OneTimeKeyboard = true,
                ResizeKeyboard = true
            };
        }
    }

    public ReplyKeyboardMarkup ConfirmationKeyboard => new([
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