using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class NewCommand : ICommand
{
    private IKeyboards Keyboards { get; }
    private IDatabaseConnection DatabaseConnection { get; }
    public string Command => "/new";
    public string Name => "Новый запрос";
    public string Description => "добавить запрос в рассылку";

    public NewCommand(IKeyboards keyboards, IDatabaseConnection databaseConnection)
    {
        Keyboards = keyboards;
        DatabaseConnection = databaseConnection;
    }

    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        if (user.Queries.Count >= Bot.MaxQueries)
            await botClient.SendMessage(chatId: user.Id,
                text: $"Вы не можете добавить больше {Bot.MaxQueries} запросов в рассылку.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        else if (user.State.EnteringQuery)
            await ProcessQueryText(botClient, user, cancellationToken, message);
        else if (user.State.ConfirmingQuery)
            await Confirm(botClient, user, cancellationToken, message);
        else
            await PromptForQuery(user, botClient, cancellationToken);
    }

    private async Task PromptForQuery(User user, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        user.State.EnteringQuery = true;
        DatabaseConnection.UpdateUserInDatabase(user);
        await botClient.SendMessage(chatId: user.Id, text: "Введите запрос для добавления в рассылку:",
            cancellationToken: cancellationToken);
    }

    private async Task ProcessQueryText(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        var query = new Query(char.ToUpper(message[0]) + message[1..].ToLower());
        user.State.EnteringQuery = false;

        if (user.Queries.Contains(query))
        {
            await botClient.SendMessage(chatId: user.Id, text: $"Запрос \"{query}\" уже есть в рассылке",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else if (message.Equals("отмена", StringComparison.CurrentCultureIgnoreCase))
        {
            await botClient.SendMessage(chatId: user.Id,
                text: "Добавление запроса отменено.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ConfirmingQuery = true;
            user.State.ProcessingQuery = query;
            await botClient.SendMessage(chatId: user.Id, text: $"Вы хотите добавить запрос '{query}' в рассылку?",
                replyMarkup: Keyboards.ConfirmationKeyboard, cancellationToken: cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(user);
    }

    private async Task Confirm(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        if (message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            user.Queries.Add(user.State.ProcessingQuery);
            user.State.ConfirmingQuery = false;
            await botClient.SendMessage(chatId: user.Id, text: "Запрос добавлен в рассылку.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            user.State.ConfirmingQuery = false;
            await botClient.SendMessage(chatId: user.Id, text: "Запрос не был добавлен.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(user);
    }
}