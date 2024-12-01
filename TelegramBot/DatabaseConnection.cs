using System.Reflection;
using System.Text.Json;
using Bot.Models;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace Bot;
static class DatabaseConnection
{
    // TODO: избавиться от static, перейти на DI
    public static RedisConnectionProvider Provider = new("redis://localhost:6379");
    public static IRedisCollection<User> Users;
    public static IConnectionMultiplexer ConnectionMultiplexer;

    static DatabaseConnection()
    {
        Users = Provider.RedisCollection<User>();
        ConnectionMultiplexer = Provider
            .GetType()
            .GetField("_mux", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(Provider) as IConnectionMultiplexer ?? throw new Exception();

        RedisSerializationSettings.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
        RedisSerializationSettings.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        var ind = RedisSerializationSettings.JsonSerializerOptions.Converters
            .Select((item, index) => (item, index))
            .First((a) => a.item.Type == typeof(DateTime))
            .index;
        RedisSerializationSettings.JsonSerializerOptions.Converters.RemoveAt(ind);

    }

    public static IEnumerable<User> PopAllUsers()
    {
        var db = ConnectionMultiplexer.GetDatabase();
        RedisValue r;
        do
        {
            r = db.SetPop("updated_users");
            if (!r.IsNullOrEmpty)
            {
                var id = (long)r;
                yield return Users.FindById(id.ToString());
            }
        } while (!r.IsNullOrEmpty);
    }
}