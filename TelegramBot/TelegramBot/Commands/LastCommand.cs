using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class LastCommand : ICommand
{
    private IKeyboards Keyboards { get; }
    private IDatabaseConnection DatabaseConnection { get; }
    public string Command => "/last";
    public string Name => "Последние статьи";
    public string Description => "показать последние статьи по запросу";

    public LastCommand(IKeyboards keyboards, IDatabaseConnection databaseConnection)
    {
        Keyboards = keyboards;
        DatabaseConnection = databaseConnection;
    }
    
    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        if (user.State.EnteringQueryToSeeLastArticles)
            await PromptForArticlesAmount(user, botClient, cancellationToken, message);
        else if (user.State.EnteringMaxArticlesToSeeLast)
            await TrySendLastArticles(user, botClient, cancellationToken, message);
        else
            await PromptForQuery(user, botClient, cancellationToken);
    }

    private async Task PromptForQuery(User user, ITelegramBotClient botClient, CancellationToken cancellationToken)
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

        user.State.EnteringQueryToSeeLastArticles = true;
        DatabaseConnection.UpdateUserInDatabase(user);
        await botClient.SendMessage(chatId: user.Id,
            text: "Введите запрос для просмотра последних новых статей.",
            replyMarkup: keyboard, cancellationToken: cancellationToken);

        if (queriesMessage.Length > 0)
            await botClient.SendMessage(chatId: user.Id,
                text: $"Вы можете как ввести новый запрос, так и выбрать один из подключённых:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task PromptForArticlesAmount(User user, ITelegramBotClient botClient, 
        CancellationToken cancellationToken, string message)
    {
        user.State.EnteringQueryToSeeLastArticles = false;

        if (message == "Отмена")
        {
            await botClient.SendMessage(chatId: user.Id, text: "Просмотр статей отменен",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ProcessingQuery = new Query(message);
            user.State.EnteringMaxArticlesToSeeLast = true;
            await botClient.SendMessage(chatId: user.Id,
                text: "Введите количество статей для просмотра (не более 25):",
                cancellationToken: cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(user);
    }

    private async Task TrySendLastArticles(User user, ITelegramBotClient botClient, 
        CancellationToken cancellationToken, string message)
    {
        if (message == "Отмена")
        {
            user.State.EnteringMaxArticlesToSeeLast = false;
            user.State.EnteringQueryToSeeLastArticles = false;
            await botClient.SendMessage(chatId: user.Id, text: "Просмотр статей отменен",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else if (int.TryParse(message, out var maxArticles) && maxArticles is > 0 and <= 25)
        {
            user.State.EnteringMaxArticlesToSeeLast = false;
            await LastArticlesGetter.SendLastArticles(botClient, user, maxArticles, cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId: user.Id, text: "Введите корректное число статей (от 1 до 25):",
                cancellationToken: cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(user);
    }
}