using Bot.TelegramBot.Commands;
using Ninject.Modules;
using Telegram.Bot;

namespace Bot.TelegramBot;

public class BotModule : NinjectModule
{
    public override void Load()
    {
        Bind<ICommandFactory>().To<CommandFactory>().InSingletonScope();
        Bind<ITelegramBotClient>().ToMethod(_ => 
            new TelegramBotClient("7377758210:AAEnYbnKCdZ5CNXzSxsN0XRl6iPZLs8BOdA")).InSingletonScope();
        
        Bind<HelpCommand>().ToSelf();
        Bind<NewCommand>().ToSelf();
        Bind<LastCommand>().ToSelf();
        Bind<RemoveCommand>().ToSelf();
    }
}