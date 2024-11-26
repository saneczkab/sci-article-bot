namespace Bot.Models;

public class Query
{
    public string Text { get; set; }
    public DateOnly LastSearch { get; set; }
    public List<Article> NewArticles { get; set; } = [];

    public Query(string text)
    {
        Text = text;
        LastSearch = DateOnly.FromDateTime(DateTime.Now);
    }

    public override string ToString() => Text;

    public override bool Equals(object? obj) => obj is Query query && query.Text == Text;
}