using Bot.TelegramBot.Commands;

using System.Diagnostics;
using System.Text.Json;
using Redis.OM;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Ninject;
using User = Bot.Models.User;

namespace Bot.TelegramBot;

public static class MessageHandler
{
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

    private static User AddUserToDatabase(long chatId)
    {
        var user = new User(chatId);
        DatabaseConnection.Users.Insert(user);
        return user;
    }

    private static User GetUserFromDatabase(long chatId)
    {
        var user = DatabaseConnection.Users.Where(u => u.Id == chatId).FirstOrDefault() ?? AddUserToDatabase(chatId);
        return user;
    }

    public static void UpdateUserInDatabase(User user)
    {
        DatabaseConnection.Users.Insert(user);
    }

    private static void RemoveUserFromDatabase(User user)
    {
        DatabaseConnection.Users.Delete(user);
    }

    public static async Task GetNewArticles(ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        // Временное решение, пока нет бд
        foreach (var user in DatabaseConnection.PopUpdatedUsers())
        {
            await botClient.SendMessage(chatId: user.Id, text: "Произошёл поиск новых статей...",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        }
    }
}