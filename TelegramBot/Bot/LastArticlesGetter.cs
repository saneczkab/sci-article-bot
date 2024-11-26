using System.Diagnostics;
using System.Text.Json;
using Bot.Bot;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class LastArticlesGetter
{
        public static async Task SendLastArticles(ITelegramBotClient botClient, User user, int message,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId: user.Id, text: "Идёт поиск статей...", 
            cancellationToken: cancellationToken);
        var query = user.State.ProcessingQuery!.Text;
        
        var articles = GetLastArticles(query, message);
        if (articles.Count == 0)
            await botClient.SendMessage(chatId: user.Id, text: "По вашему запросу не найдено статей",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        else
        {
            var response = articles.Aggregate($"Последние статьи по запросу {query}:\n", 
                (current, article) => 
                    current + $"- [{article.Title}]({article.URL}) ({article.PublicationDate})\n");

            await botClient.SendMessage(chatId: user.Id, text: response, 
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: Keyboards.CommandsKeyboard,
                cancellationToken: cancellationToken);
        }
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