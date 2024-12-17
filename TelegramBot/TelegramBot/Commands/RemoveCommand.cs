using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class RemoveCommand : ICommand
{
    private IKeyboards Keyboards { get; }
    public string Command => "/remove";
    public string Name => "Удалить запрос";
    public string Description => "удалить запрос из рассылки";

    public RemoveCommand(IKeyboards keyboards)
    {
        Keyboards = keyboards;
    }
    
    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        if (user.State.RemovingQuery)
            await ProcessQueryText(user, botClient, cancellationToken, message);
        else if (user.State.ConfirmingRemoval)
            await Confirm(user, botClient, cancellationToken, message);
        else
            await PromptForQueryChoosing(user, botClient, cancellationToken);
    }

    private async Task ProcessQueryText(User user, ITelegramBotClient botClient, 
        CancellationToken cancellationToken, string message)
    {
        user.State.RemovingQuery = false;
        var query = user.Queries
            .FirstOrDefault(q => q.ToString().Equals(message, StringComparison.CurrentCultureIgnoreCase));

        if (message == "Отмена")
        {
            await botClient.SendMessage(chatId: user.Id, text: "Удаление запроса отменено",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else if (query is null)
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{message}\" не найден",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ConfirmingRemoval = true;
            user.State.ProcessingQuery = query;
            await botClient.SendMessage(chatId: user.Id, text: $"Удалить запрос \"{query}\"?",
                replyMarkup: Keyboards.ConfirmationKeyboard, cancellationToken: cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(user);
    }

    private async Task Confirm(User user, ITelegramBotClient botClient, 
        CancellationToken cancellationToken, string message)
    {
        user.State.ConfirmingRemoval = false;
        var query = user.State.ProcessingQuery;

        if (message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            user.Queries.Remove(query!);
            DatabaseConnection.UpdateUserInDatabase(user);
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" удален",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" не будет удален",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
    }

    private async Task PromptForQueryChoosing(User user, ITelegramBotClient botClient, 
        CancellationToken cancellationToken)
    {
        var queries = user.Queries;
        var queriesMessage = queries.Aggregate("", (current, query) => current + $"{query}\n");
        var buttons = queries
            .Select(query => new KeyboardButton(query.ToString()))
            .Append(new KeyboardButton("Отмена"));
        var keyboard = new ReplyKeyboardMarkup(buttons)
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };

        if (queriesMessage.Length == 0)
        {
            await botClient.SendMessage(chatId: user.Id, text: "У Вас нет запросов в рассылке",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.RemovingQuery = true;
            DatabaseConnection.UpdateUserInDatabase(user);
            await botClient.SendMessage(chatId: user.Id, text: $"Выберите запрос для удаления:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
    }
}