using System.Diagnostics;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot;

public class ArticlesGetter : IQueryHandler
{
    public static async Task Handle(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
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
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.EnteringQueryToSeeLastArticles = true;
            MessageHandler.UpdateUserInDatabase(user);
            await botClient.SendMessage(chatId: user.Id, 
                text: $"Выберите запрос для просмотра последних пяти новых статей:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
    }

    public static async Task GetText(ITelegramBotClient botClient, User user, string message, CancellationToken cancellationToken)
    {
        user.State.EnteringQueryToSeeLastArticles = false;
        var query = user.Queries
            .FirstOrDefault(q => q.ToString().Equals(message, StringComparison.CurrentCultureIgnoreCase));

        if (message == "Отмена")
        {
            await botClient.SendMessage(chatId: user.Id, text: "Просмотр статей отменен",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else if (query is null)
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{message}\" не найден",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await MessageHandler.SendLastArticles(botClient, user, query.Text, cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(user);
    }

    public static Task GetConfirmation(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken) => throw new NotImplementedException();
}