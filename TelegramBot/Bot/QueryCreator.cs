using Telegram.Bot;

namespace Bot.TelegramBot;

public class QueryCreator : IQueryHandler
{
    public static async Task Handle(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken)
    {
        user.State.EnteringQuery = true;
        MessageHandler.UpdateUserInDatabase(user);
        const string message = "Введите поисковый запрос для новой рассылки";

        await botClient.SendMessage(chatId: user.Id, text: message, cancellationToken: cancellationToken);
    }

    public static async Task GetText(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken)
    {
        var query = new Query(message);
        user.State.EnteringQuery = false;
        await MessageHandler.SendLastArticles(botClient, user, message, cancellationToken);

        if (user.Queries.Contains(query))
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" уже есть в рассылке",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ConfirmingQuery = true;
            user.State.ProcessingQuery = query;
            await botClient.SendMessage(chatId: user.Id, text: $"Добавить запрос \"{query}\" в рассылку?",
                replyMarkup: MessageHandler.ConfirmationKeyboard, cancellationToken: cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(user);
    }

    public static async Task GetConfirmation(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken)
    {
        user.State.ConfirmingQuery = false;
        var query = user.State.ProcessingQuery;

        if (message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            user.Queries.Add(user.State.ProcessingQuery!);
            await botClient
                .SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" добавлен в рассылку",
                    replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient
                .SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" не будет добавлен в рассылку",
                    replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(user);
    }
}