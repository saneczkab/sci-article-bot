using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Bot.TelegramBot;
using Telegram.Bot;

public class ArticleProcessor
{
    private readonly IDatabaseConnection _database;
    private Timer? _dailyTimer;

    public ArticleProcessor(IDatabaseConnection database)
    {
        _database = database;
    }

    public void HandleUsers()
    {
        Console.WriteLine("Handling users...");
        var users = _database.GetAllUsers().ToList();

        foreach (var user in users)
        {
            foreach (var q in user.Queries)
            {
                q.NewArticles.Clear();
            }
            
            _database.UpdateUserInDatabase(user);
        }
        Console.WriteLine("Cleared new articles for all users.");

        foreach (var user in users)
            HandleUser(user);
        Console.WriteLine("Users handled.");
    }

    private void HandleUser(User user)
    {
        Console.WriteLine($"Handling user {user.Id}...");
        var newArticlesFound = false;

        foreach (var query in user.Queries)
        {
            query.LastSearch = DateTime.UtcNow;

            Console.WriteLine($"Searching for articles for query '{query.Text}'...");
            var articles = SearchEngine.SearchTodayArticles(query.Text);
            var filteredArticles = Filter.FilterArticles(articles).ToList();
            Console.WriteLine($"Found {filteredArticles.Count} filtered articles and {articles.Count} total articles for query '{query.Text}'");
            
            foreach (var article in filteredArticles)
            {
                //if (user.ShownArticlesDois.Contains(article.Doi))
                //    continue;
                
                if (string.IsNullOrWhiteSpace(article.Title))
                {
                    Console.WriteLine($"[WARNING] Article without title found for query '{query.Text}' — skipped.");
                    continue;
                }

                newArticlesFound = true;
                query.NewArticles.Add(article);
                user.ShownArticlesDois.Add(article.Doi);
            }
        }

        _database.UpdateUserInDatabase(user);
        _database.MarkUserAsUpdated(user.Id);
    }

    public void ScheduleDailyTask(TimeSpan time, ITelegramBotClient botClient, MessageHandler messageHandler)
    {
        var now = DateTime.Now;
        var firstRun = now.Date.Add(time);
        if (firstRun < now)
            firstRun = firstRun.AddDays(1);

        var delay = firstRun - now;
        var period = TimeSpan.FromDays(1);

        _dailyTimer = new Timer(_ =>
        {
            HandleUsers();
            messageHandler.GetNewArticles(botClient, CancellationToken.None).Wait();
        }, null, delay, period);
    }

    public static void BlockingRun()
    {
        while (true)
            Thread.Sleep(60000);
    }
}