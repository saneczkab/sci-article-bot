using Bot.TelegramBot.Commands;
using Bot.TelegramBot.Interfaces;
using Ninject;
using Ninject.Modules;

namespace Bot.TelegramBot;

public class BotModule : NinjectModule
{
    public override void Load()
    {
        Bind<IKeyboards>().To<Keyboards>().InSingletonScope();
        Bind<IEnumerable<ICommand>>().ToMethod(ctx => ctx.Kernel.GetAll<ICommand>()).InSingletonScope();
        Bind<IDatabaseConnection>().To<DatabaseConnection>().InSingletonScope();
        
        Bind<ICommandFactory>().To<CommandFactory>().InSingletonScope();
        Bind<ICommand>().To<HelpCommand>();
        Bind<ICommand>().To<NewCommand>();
        // Bind<ICommand>().To<LastCommand>();
        Bind<ICommand>().To<RemoveCommand>();
        
        Bind<MessageHandler>().ToSelf().InSingletonScope();
        Bind<LastArticlesGetter>().ToSelf().InSingletonScope();
        Bind<ArticleProcessor>().ToSelf().InSingletonScope();
    }
}