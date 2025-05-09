using System.Reflection;
using System.Text.Json;
using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace Bot;
public class DatabaseConnection : IDatabaseConnection
{
    private RedisConnectionProvider Provider = new("redis://localhost:6379");
    private IRedisCollection<User> Users;
    private IConnectionMultiplexer ConnectionMultiplexer;

    private DatabaseConnection()
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

    public IEnumerable<User> PopAllUsers()
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
    
    public User AddUserToDatabase(long chatId)
    {
        var user = new User(chatId);
        Users.Insert(user);
        return user;
    }

    public User GetUserFromDatabase(long chatId)
    {
        var user = Users.FindById(chatId.ToString()) ?? AddUserToDatabase(chatId);
        return user;
    }

    public void UpdateUserInDatabase(User user)
    {
        Users.Insert(user);
    }

    public void RemoveUserFromDatabase(User user)
    {
        Users.Delete(user);
    }
    
    public void MarkUserAsUpdated(long chatId)
    {
        var db = ConnectionMultiplexer.GetDatabase();
        db.SetAdd("updated_users", chatId);
    }
}