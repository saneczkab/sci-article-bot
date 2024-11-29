namespace Bot;

public record Query(string Text, DateTime? LastSearch = null)
{
    public List<Article> NewArticles { get; set; } = [];
    public override string ToString() => Text;
}