namespace Bot.Models;

public class User
{
    public long Id { get; set; }
    public List<Query> Queries { get; set; } = [];
    public HashSet<string> ShownArticlesDois { get; set; } = [];

    public UserState State
    {
        get
        {
            States.TryAdd(Id, new());
            return States[Id];
        }
    }

    public User(long id)
    {
        Id = id;
    }

    private static readonly Dictionary<long, UserState> States = [];
}
