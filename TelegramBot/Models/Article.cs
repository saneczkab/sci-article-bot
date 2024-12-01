using System.Globalization;
using System.Text;
using Redis.OM.Modeling;

namespace Bot.Models;

[Document(StorageType = StorageType.Json)]
public class Article
{
    public required string Title { get; set; }
    public string? Doi { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public DateOnly? Issued { get; set; }
    public Journal? Journal { get; set; }
    public string? Url { get; set; }

    public override string ToString()
    {
        var result = new List<string>() { $"<b>\"{Title}\"</b>" };
        if (Author is not null)
            result.Add($"({Author})");
        result.Add("опубликована");
        if (Issued is not null)
            result.Add(Issued.Value.ToString("d MMMM yyyy", new CultureInfo("ru")));
        if (Journal is not null)
        {
            if (Url is not null)
                result.Add($"в журнале <a href=\"{Url}\"><i>{Journal.Title}</i></a>");
            else
                result.Add($"в журнале <i>{Journal.Title}</i>");
        }

        return string.Join(' ', result);
    }
}