using System.Reflection;
using Bot;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace Bot;
static class DatabaseConnection
{
    public static RedisConnectionProvider Provider = new("redis://localhost:6379");
    public static IRedisCollection<User> Users;
    public static IConnectionMultiplexer ConnectionMultiplexer;

    static DatabaseConnection()
    {
        Users = Provider.RedisCollection<User>();
        ConnectionMultiplexer = Provider
            .GetType()
            .GetRuntimeField("_mux")
            .GetValue(Provider) as IConnectionMultiplexer ?? throw new Exception();
    }

    public static IEnumerable<User> PopUpdatedUsers()
    {
        var db = ConnectionMultiplexer.GetDatabase();

        RedisValue r;
        do
        {
            r = db.SetPop("updated_users");
            if (!r.IsNullOrEmpty)
            {
                var id = (long)r;
                yield return Users.Where(u => u.Id == id).First();
            }
        } while (!r.IsNullOrEmpty);
    }
}