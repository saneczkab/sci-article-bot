using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Bot.TelegramBot;

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
        var users = _database.GetAllUsers().ToList();

        foreach (var user in users)
            HandleUser(user);
    }

    private void HandleUser(User user)
    {
        var newArticlesFound = false;

        foreach (var query in user.Queries)
        {
            query.LastSearch = DateTime.UtcNow;

            var articles = SearchEngine.SearchTodayArticles(query.Text);
            var filteredArticles = Filter.FilterArticles(articles).ToList();
            
            foreach (var article in filteredArticles)
            {
                if (user.ShownArticlesDois.Contains(article.Doi))
                    continue;

                newArticlesFound = true;
                query.NewArticles.Add(article);
                user.ShownArticlesDois.Add(article.Doi);
            }
        }

        _database.UpdateUserInDatabase(user);
        _database.MarkUserAsUpdated(user.Id);
    }

    public void ScheduleDailyTask(TimeSpan time)
    {
        var now = DateTime.Now;
        var firstRun = now.Date.Add(time);
        if (firstRun < now)
            firstRun = firstRun.AddDays(1);

        var delay = firstRun - now;
        var period = TimeSpan.FromDays(1);

        _dailyTimer = new Timer(_ => HandleUsers(), null, delay, period);
    }

    public static void BlockingRun()
    {
        while (true)
            Thread.Sleep(60000);
    }
}