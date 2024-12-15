using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Interfaces;

public interface IKeyboards
{
    public ReplyKeyboardMarkup CommandsKeyboard { get; }
    public ReplyKeyboardMarkup ConfirmationKeyboard { get; }
}