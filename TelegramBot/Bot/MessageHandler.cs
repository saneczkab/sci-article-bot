using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Bot.TelegramBot.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Ninject;
using Ninject.Parameters;

namespace Bot.TelegramBot;

public static class MessageHandler
{
    private static readonly ConcurrentDictionary<long, User> Users = new(); // Временное решение, пока нет бд.
    private static readonly IKernel Kernel = new StandardKernel(new BotModule());
    
    public static readonly ReplyKeyboardMarkup CommandsKeyboard = new([
        [
            new KeyboardButton("/help"),
            new KeyboardButton("/new"),
            new KeyboardButton("/last"),
            new KeyboardButton("/remove")
        ]
    ])
    {
        OneTimeKeyboard = true,
        ResizeKeyboard = true
    };
    
    public static readonly ReplyKeyboardMarkup ConfirmationKeyboard = new([
        [
            new KeyboardButton("Да"),
            new KeyboardButton("Нет")
        ]
    ])
    {
        OneTimeKeyboard = true,
        ResizeKeyboard = true
    };

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
        var commandFactory = Kernel.Get<ICommandFactory>();

        try
        {
            var command = commandFactory.CreateCommand(user, text, cancellationToken);
            await command.Execute();
        }
        catch (InvalidOperationException)
        {
            await botClient.SendMessage(chatId: user.Id, 
                text: "Неизвестная команда. Для получения списка команд введите /help", 
                replyMarkup: CommandsKeyboard, cancellationToken: cancellationToken);
        }
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
                replyMarkup: CommandsKeyboard, cancellationToken: cancellationToken);
        }
    }
}