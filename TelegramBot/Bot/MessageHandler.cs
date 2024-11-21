using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot;

public static class MessageHandler
{
    private static readonly ConcurrentDictionary<long, User> Users = new(); // Временное решение, пока нет бд.
    
    private const string HelpMessage = "В боте доступны следующие команды:\n" +
                                       "/help - список доступных команд\n" +
                                       "/new - добавить новый запрос в рассылку\n" +
                                       "/last - показать последние 5 опубликованных статей для запроса\n" +
                                       "/remove - удалить из рассылки один из запросов\n";
    
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
        if (user.State.EnteringQuery)
            await QueryCreator.GetText(botClient, user, message.Text!, cancellationToken);
        else if (user.State.ConfirmingQuery)
            await QueryCreator.GetConfirmation(botClient, user, message.Text!, cancellationToken);
        else if (user.State.RemovingQuery)
            await QueryRemover.GetText(botClient, user, message.Text!, cancellationToken);
        else if (user.State.ConfirmingRemoval)
            await QueryRemover.GetConfirmation(botClient, user, message.Text!, cancellationToken);
        else if (user.State.EnteringQueryToSeeLastArticles)
            await ArticlesGetter.GetText(botClient, user, message.Text!, cancellationToken);
        else
        {
            await (message.Text switch
            {
                "/start" => SendGreetingMessage(botClient, user, cancellationToken),
                "/help" => SendHelpMessage(botClient, user, cancellationToken),
                "/new" => QueryCreator.Handle(botClient, user, cancellationToken),
                "/last" => ArticlesGetter.Handle(botClient, user, cancellationToken),
                "/remove" => QueryRemover.Handle(botClient, user, cancellationToken),
                _ => botClient.SendMessage(chatId: user.Id,
                    text: "Неизвестная команда. Для получения списка команд введите /help",
                    replyMarkup: CommandsKeyboard, cancellationToken: cancellationToken)
            });
        }
    }

    public static Task HandleError(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception}");
        return Task.CompletedTask;
    }
    
    private static async Task SendGreetingMessage(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken)
    {
        const string text = "Добро пожаловать в SciArticleBot - бот для отслеживания новых научных статей.\n" +
                            "Раз в сутки бот проверяет наличие новых статей в одобренных РЦНИ журналах " +
                            "и отправляет Вам.\n" + HelpMessage;

        await botClient.SendMessage(chatId: user.Id, text: text, replyMarkup: CommandsKeyboard,
            cancellationToken: cancellationToken);
    }
    
    private static async Task SendHelpMessage(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId: user.Id, text: HelpMessage, replyMarkup: CommandsKeyboard,
                cancellationToken: cancellationToken);
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
    
    public static async Task SendLastArticles(ITelegramBotClient botClient, User user, string message,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId: user.Id, text: "Идёт поиск статей...", 
            cancellationToken: cancellationToken);
        
        var articles = GetLastArticles(message, 5);
        if (articles.Count == 0)
            await botClient.SendMessage(chatId: user.Id, text: "По вашему запросу не найдено статей",
                replyMarkup: CommandsKeyboard, cancellationToken: cancellationToken);

        var response = articles.Aggregate($"Последние статьи по запросу {message}:\n",
            (current, article) => current + $"- [{article.Title}]({article.URL}) ({article.PublicationDate})\n");

        await botClient.SendMessage(chatId: user.Id, text: response, 
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: CommandsKeyboard,
            cancellationToken: cancellationToken);
    }

    private static List<Article> GetLastArticles(string query, int maxArticles)
    {
        var scriptPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TempScripts", "scrapper.py");
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{Path.GetFullPath(scriptPath)}\" \"{query}\" {maxArticles}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine(error);
        var result = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(output);

        return (from item in result
            let title = item["title"].ToString()
            let depositedDate =
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(item["deposited_date"].ToString())).DateTime
            let issn = item["issn"].ToString()
            let url = item["url"].ToString()
            select new Article(title, DateOnly.FromDateTime(depositedDate), issn, url)).ToList();
    }
}