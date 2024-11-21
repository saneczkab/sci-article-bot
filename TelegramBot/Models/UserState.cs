namespace Bot;

public class UserState
{
    public bool EnteringQuery { get; set; }
    public bool ConfirmingQuery { get; set; }
    public bool RemovingQuery { get; set; }
    public bool ConfirmingRemoval { get; set; }
    public bool EnteringQueryToSeeLastArticles { get; set; }
    public Query? ProcessingQuery { get; set; } 
}