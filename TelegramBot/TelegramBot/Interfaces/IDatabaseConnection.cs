using Bot.Models;

namespace Bot.TelegramBot.Interfaces;

public interface IDatabaseConnection
{
    public IEnumerable<User> PopAllUsers();
    public User AddUserToDatabase(long chatId);
    public User GetUserFromDatabase(long chatId);
    public void UpdateUserInDatabase(User user);
    public void RemoveUserFromDatabase(User user);
}