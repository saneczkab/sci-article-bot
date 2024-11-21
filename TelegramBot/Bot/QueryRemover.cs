using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot;

public class QueryRemover : IQueryHandler
{
    public static async Task Handle(ITelegramBotClient botClient, User user, CancellationToken cancelToken)
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
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancelToken);
        }
        else
        {
            user.State.RemovingQuery = true;
            MessageHandler.UpdateUserInDatabase(user);
            await botClient.SendMessage(chatId: user.Id, text: $"Выберите запрос для удаления:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: cancelToken);
        }
    }

    public static async Task GetText(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken)
    {
        user.State.RemovingQuery = false;
        var query = user.Queries
            .FirstOrDefault(q => q.ToString().Equals(message, StringComparison.CurrentCultureIgnoreCase));

        if (message == "Отмена")
        {
            await botClient.SendMessage(chatId: user.Id, text: "Удаление запроса отменено",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else if (query is null)
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{message}\" не найден",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ConfirmingRemoval = true;
            user.State.ProcessingQuery = query;
            await botClient.SendMessage(chatId: user.Id, text: $"Удалить запрос \"{query}\"?",
                replyMarkup: MessageHandler.ConfirmationKeyboard, cancellationToken: cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(user);
    }

    public static async Task GetConfirmation(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken)
    {
        user.State.ConfirmingRemoval = false;
        var query = user.State.ProcessingQuery;
        MessageHandler.UpdateUserInDatabase(user);

        if (message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            user.Queries.Remove(query!);
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" удален",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" не будет удален",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
    }
}