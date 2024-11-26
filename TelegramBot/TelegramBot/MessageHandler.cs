using System.Collections.Concurrent;
using Bot.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Ninject;
using User = Bot.Models.User;

namespace Bot.TelegramBot;

public static class MessageHandler
{
    private static readonly ConcurrentDictionary<long, User> Users = new(); // Временное решение, пока нет бд.

    public static async Task HandleUpdate(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message is null)
            return;

        var chatId = message.Chat.Id;
        var user = GetUserFromDatabase(chatId);

        try
        {
            await SendResponse(botClient, user, message, cancellationToken);
        }
        catch (ApiRequestException apiEx) when (apiEx.ErrorCode == 403)
        {
            RemoveUserFromDatabase(user);
        }
    }

    private static async Task SendResponse(ITelegramBotClient botClient, User user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text!;
        var commandFactory = KernelHandler.Kernel.Get<CommandFactory>();
        var command = commandFactory.CreateCommand(user, text, cancellationToken);
        await command.Execute(botClient, user, cancellationToken, text);
    }


    public static Task HandleError(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception}");
        return Task.CompletedTask;
    }

    private static void AddUserToDatabase(long chatId)
    {
        // Временное решение, пока нет бд
        Users.TryAdd(chatId, new User(chatId));
    }

    private static User GetUserFromDatabase(long chatId)
    {
        // Временное решение, пока нет бд
        var isUserExists = Users.TryGetValue(chatId, out _);
        if (!isUserExists)
            AddUserToDatabase(chatId);

        return Users[chatId];
    }

    public static void UpdateUserInDatabase(User user)
    {
        // Временное решение, пока нет бд
        Users[user.Id] = user;
    }

    private static void RemoveUserFromDatabase(User user)
    {
        // Временное решение, пока нет бд
        Users.TryRemove(user.Id, out _);
    }

    public static async Task GetNewArticles(ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        // Временное решение, пока нет бд
        foreach (var user in Users)
        {
            await botClient.SendMessage(chatId: user.Key, text: "Произошёл поиск новых статей...",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
    }
}