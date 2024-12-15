using Bot.TelegramBot.Commands;

using System.Diagnostics;
using System.Text.Json;
using Redis.OM;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Ninject;
using User = Bot.Models.User;
using System.Text;
using Bot.TelegramBot.Interfaces;

namespace Bot.TelegramBot;

public class MessageHandler
{
    private static ICommandFactory _factory;
    private IKeyboards Keyboards { get; }

    public MessageHandler(ICommandFactory factory, IKeyboards keyboards)
    {
        _factory = factory;
        Keyboards = keyboards;
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
        var commandFactory = _factory;
        var command = commandFactory.CreateCommand(user, text, cancellationToken);
        
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
    
    public async Task GetNewArticles(ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        foreach (var user in DatabaseConnection.PopAllUsers())
        {
            var message = new StringBuilder();
            foreach (var query in user.Queries)
            {
                if (query.NewArticles.Count == 0)
                {
                    message.Append($"По запросу <code>{query.Text}</code> не найдено ни одной статьи.\n\n");
                    continue;
                }

                message.Append($"По запросу <code>{query.Text}</code> найдены следующие статьи:\n\n");
                for (var i = 0; i < query.NewArticles.Count; i++)
                    message.Append($"{i + 1}. {query.NewArticles[i]}\n\n");
                message.Append('\n');

                query.NewArticles.Clear();
            }
            DatabaseConnection.UpdateUserInDatabase(user);
            await botClient.SendMessage(chatId: user.Id, text: message.ToString(),
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}