using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using User = Bot.Models.User;
using System.Text;
using Bot.TelegramBot.Interfaces;

namespace Bot.TelegramBot;

public class MessageHandler
{
    private ICommandFactory Factory { get; }
    private IKeyboards Keyboards { get; }
    private IDatabaseConnection DatabaseConnection { get; }

    public MessageHandler(ICommandFactory factory, IKeyboards keyboards, IDatabaseConnection databaseConnection)
    {
        Factory = factory;
        Keyboards = keyboards;
        DatabaseConnection = databaseConnection;
    }
    
    public async Task HandleUpdate(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message is null)
            return;

        var chatId = message.Chat.Id;
        var user = DatabaseConnection.GetUserFromDatabase(chatId);

        try
        {
            await SendResponse(botClient, user, message, cancellationToken);
        }
        catch (ApiRequestException apiEx) when (apiEx.ErrorCode == 403)
        {
            DatabaseConnection.RemoveUserFromDatabase(user);
        }
    }

    private async Task SendResponse(ITelegramBotClient botClient, User user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text!;
        var command = Factory.GetCommand(user, text, cancellationToken);
        
        if (command is null)
        {
            await botClient.SendMessage(chatId: user.Id,
                text: "Неизвестная команда. Для получения списка команд введите /help",
                cancellationToken: cancellationToken, replyMarkup: Keyboards.CommandsKeyboard);
            return;
        }
        await command.Execute(botClient, user, cancellationToken, text);
    }
    
    public Task HandleError(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception}");
        return Task.CompletedTask;
    }
    
public async Task GetNewArticles(ITelegramBotClient botClient, CancellationToken cancellationToken)
{
    Console.WriteLine("GetNewArticles Handling users...");
    foreach (var user in DatabaseConnection.PopAllUsers())
    {
        Console.WriteLine($"GetNewArticles Handling user {user.Id}...");
        var messages = new List<string>();
        var builder = new StringBuilder();

        foreach (var query in user.Queries)
        {
            if (query.NewArticles.Count == 0)
            {
                var section = $"По запросу <code>{query.Text}</code> за последние сутки не найдено ни одной новой статьи.\n\n";
                if (builder.Length + section.Length > 4000)
                {
                    messages.Add(builder.ToString());
                    builder.Clear();
                }
                builder.Append(section);
                continue;
            }

            var sectionHeader = $"По запросу <code>{query.Text}</code> найдены следующие статьи:\n\n";
            if (builder.Length + sectionHeader.Length > 4000)
            {
                messages.Add(builder.ToString());
                builder.Clear();
            }
            builder.Append(sectionHeader);

            for (int i = 0; i < query.NewArticles.Count; i++)
            {
                var articleText = $"{i + 1}. {query.NewArticles[i]}\n\n";
                if (builder.Length + articleText.Length > 4000)
                {
                    messages.Add(builder.ToString());
                    builder.Clear();
                    builder.Append($"{i + 1}. {query.NewArticles[i]}\n\n");
                }
                else
                {
                    builder.Append(articleText);
                }
            }

            builder.Append('\n');
            query.NewArticles.Clear();
        }

        if (builder.Length > 0)
            messages.Add(builder.ToString());

        DatabaseConnection.UpdateUserInDatabase(user);

        foreach (var part in messages)
        {
            try
            {
                await botClient.SendMessage(chatId: user.Id, text: part,
                    replyMarkup: Keyboards.CommandsKeyboard,
                    cancellationToken: cancellationToken,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            catch (ApiRequestException apiEx) when (apiEx.ErrorCode == 403)
            {
                DatabaseConnection.RemoveUserFromDatabase(user);
                break;
            }
        }
    }
    Console.WriteLine("GetNewArticles Users handled.");
}

}