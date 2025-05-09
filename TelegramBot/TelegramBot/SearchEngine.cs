using System.Text.Json;
using Bot.Models;

public static class SearchEngine
{
    private static readonly HttpClient Client = new();

    public static List<Article> SearchTodayArticles(string query)
    {
        var now = DateTime.UtcNow.Date;
        var yesterday = now.AddDays(-1);
        var url = $"https://api.crossref.org/works?query={Uri.EscapeDataString(query)}" +
                  $"&filter=from-online-pub-date:{yesterday:yyyy-MM-dd},until-online-pub-date:{now:yyyy-MM-dd}";

        var response = Client.GetStringAsync(url).Result;
        var root = JsonDocument.Parse(response).RootElement;
        var items = root.GetProperty("message").GetProperty("items");
        var articles = new List<Article>();

        foreach (var item in items.EnumerateArray())
        {
            var journal = FormatJournal(item);

            var article = new Article
            {
                Title = item.TryGetProperty("title", out var titles) && titles.GetArrayLength() > 0
                    ? titles[0].GetString()
                    : null,
                Doi = item.TryGetProperty("DOI", out var doi) ? doi.GetString() : null,
                Author = FormatAuthor(item),
                Publisher = item.TryGetProperty("publisher", out var publisher) ? publisher.GetString() : null,
                Issued = ParseIssuedDate(item),
                Journal = journal,
                Url = item.TryGetProperty("URL", out var urlProp) ? urlProp.GetString() : null
            };

            articles.Add(article);
        }

        return articles;
    }

    private static Journal? FormatJournal(JsonElement item)
    {
        var journalTitle = 
            item.TryGetProperty("container-title", out var containerTitles) 
            && containerTitles.GetArrayLength() > 0
            ? containerTitles[0].GetString()
            : null;

        if (string.IsNullOrEmpty(journalTitle)) 
            return null;
        
        string? printIssn = null;
        string? electronicIssn = null;

        if (item.TryGetProperty("issn-type", out var issnTypes))
        {
            foreach (var issn in issnTypes.EnumerateArray())
            {
                var type = issn.GetProperty("type").GetString();
                var value = issn.GetProperty("value").GetString();
                switch (type)
                {
                    case "print":
                        printIssn = value;
                        break;
                    case "electronic":
                        electronicIssn = value;
                        break;
                }
            }
        }

        return new Journal
        {
            Title = journalTitle,
            PrintIssn = printIssn,
            ElectronicIssn = electronicIssn
        };
    }

    private static string? FormatAuthor(JsonElement item)
    {
        if (!item.TryGetProperty("author", out var authors)) 
            return null;

        var parts = new List<string>();
        foreach (var author in authors.EnumerateArray())
        {
            var family = author.TryGetProperty("family", out var fam) ? fam.GetString() : "";
            var given = author.TryGetProperty("given", out var giv) ? giv.GetString() : "";
            var full = $"{family} {given}".Trim();
            if (!string.IsNullOrEmpty(full))
                parts.Add(full);
        }

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    private static DateOnly? ParseIssuedDate(JsonElement item)
    {
        if (!item.TryGetProperty("issued", out var issued) ||
            !issued.TryGetProperty("date-parts", out var dateParts) ||
            dateParts.GetArrayLength() == 0 ||
            dateParts[0].GetArrayLength() == 0)
            return null;

        var parts = dateParts[0];
        var year = parts.GetArrayLength() > 0 ? parts[0].GetInt32() : 1;
        var month = parts.GetArrayLength() > 1 ? parts[1].GetInt32() : 1;
        var day = parts.GetArrayLength() > 2 ? parts[2].GetInt32() : 1;

        try
        {
            return new DateOnly(year, month, day);
        }
        catch
        {
            return null;
        }
    }
}
