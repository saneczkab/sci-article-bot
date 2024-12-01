using Redis.OM.Modeling;

namespace Bot.Models;


[Document(StorageType = StorageType.Json, Prefixes = [":User"])]
public class User
{
    [RedisIdField] public long Id { get; init; }

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
