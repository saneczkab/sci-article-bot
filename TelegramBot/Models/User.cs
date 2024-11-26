namespace Bot.Models;

public class User
{
    public long Id { get; set; }
    public List<Query> Queries { get; set; }
    public UserState State { get; set; }

    public User(long id)
    {
        Id = id;
        Queries = [];
        State = new UserState();
    }
}