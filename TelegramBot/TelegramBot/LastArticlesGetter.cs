using System.Diagnostics;
using System.Text.Json;
using Bot.Models;
using Telegram.Bot;

namespace Bot.TelegramBot;

public class LastArticlesGetter
{
    public static async Task SendLastArticles(ITelegramBotClient botClient, User user, int message,
        CancellationToken cancellationToken)
    {
        await botClient.SendMessage(chatId: user.Id, text: "Идёт поиск статей...",
            cancellationToken: cancellationToken);
        // TODO: реализовать
        return;
        /* var query = user.State.ProcessingQuery!.Text;

        var articles = GetLastArticles(query, message);
        if (articles.Count == 0)
            await botClient.SendMessage(chatId: user.Id, text: "По вашему запросу не найдено статей",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: cancellationToken);
        else
        {
            var response = articles.Aggregate($"Последние статьи по запросу {query}:\n",
                (current, article) =>
                    current + $"- {article}\n");

            await botClient.SendMessage(chatId: user.Id, text: response,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: Keyboards.CommandsKeyboard,
                cancellationToken: cancellationToken);
        } */
    }

    private static List<Article> GetLastArticles(string query, int maxArticles)
    {
        return [];
    }
}