using Redis.OM.Modeling;

namespace Bot;


[Document(StorageType = StorageType.Json, Prefixes = new[] { ":User" })]
public class User
{
    [RedisIdField][Indexed] public long Id { get; init; }

    public User(long id)
    {
        Id = id;
    }

    public List<Query> Queries { get; set; } = new List<Query>();

    public HashSet<string> ShownArticlesDois { get; set; } = new HashSet<string>();

    public UserState State
    {
        get
        {
            States.TryAdd(Id, new());
            return States[Id];
        }
        set => States[Id] = value;
    }

    private static readonly Dictionary<long, UserState> States = [];
}

