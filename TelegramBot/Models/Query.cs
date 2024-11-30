namespace Bot.Models;

public class Query
{
    public string Text { get; set; }
    public DateOnly? LastSearch { get; set; } = null;
    public List<Article> NewArticles { get; set; } = [];

    public Query(string text)
    {
        Text = text;
    }

    public override string ToString() => Text;
}