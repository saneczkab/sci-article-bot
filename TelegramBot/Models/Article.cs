namespace Bot.Models;

public class Article
{
    public string Title { get; set; }
    public DateOnly PublicationDate { get; set; }
    public string JournalISSN { get; set; }
    public string URL { get; set; }
    
    public Article(string title, DateOnly publicationDate ,string journalISSN, string url)
    {
        Title = title;
        PublicationDate = publicationDate;
        JournalISSN = journalISSN;
        URL = url;
    }
}