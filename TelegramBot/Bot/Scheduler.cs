namespace Bot.TelegramBot;

public static class Scheduler
{
    public static void RunDailyTask(int hour, int minute, Func<Task> task)
    {
        var now = DateTime.Now;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        if (now > nextRun)
            nextRun = nextRun.AddDays(1);

        var timeToGo = nextRun - now;
        Task.Delay(timeToGo).ContinueWith(async _ =>
        {
            await task();
            while (true)
            {
                await Task.Delay(TimeSpan.FromDays(1));
                await task();
            }
        });
    }
}