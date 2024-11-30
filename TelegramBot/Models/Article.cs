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
}