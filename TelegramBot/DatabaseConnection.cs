using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bot;

public class DatabaseConnection : IDatabaseConnection
{
    private readonly AppDbContext _context;

    public DatabaseConnection()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }

    
    public IEnumerable<User> PopAllUsers() => GetAllUsers();

    public User AddUserToDatabase(long chatId)
    {
        var user = new User(chatId);
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public User GetUserFromDatabase(long chatId)
    {
        var user = _context.Users
            .Include(u => u.Queries)
            .ThenInclude(q => q.NewArticles)
            .FirstOrDefault(u => u.Id == chatId);

        return user ?? AddUserToDatabase(chatId);
    }

    public void UpdateUserInDatabase(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void RemoveUserFromDatabase(User user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    public void MarkUserAsUpdated(long chatId)
    {
        // заглушка
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _context.Users
            .Include(u => u.Queries)
            .ThenInclude(q => q.NewArticles)
            .ToList();
    }
}