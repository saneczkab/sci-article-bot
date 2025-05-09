using Bot.Models;

namespace Bot.TelegramBot;

public static class Filter
{
    private static readonly HashSet<string> ApprovedIssns = new();

    static Filter()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "issns.txt");
        foreach (var line in File.ReadLines(path))
            ApprovedIssns.Add(line.Trim());
    }

    public static IEnumerable<Article> FilterArticles(IEnumerable<Article> articles) => articles.Where(article =>
        article.Journal is not null &&
        ((article.Journal.PrintIssn is not null && ApprovedIssns.Contains(article.Journal.PrintIssn)) ||
         (article.Journal.ElectronicIssn is not null && ApprovedIssns.Contains(article.Journal.ElectronicIssn))));
}