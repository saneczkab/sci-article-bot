using System.Reflection;
using Bot.TelegramBot.Commands;
using Ninject.Modules;
using Telegram.Bot;

namespace Bot.TelegramBot;

public class BotModule : NinjectModule
{
    public override void Load()
    {
        Bind<ICommandFactory>().To<CommandFactory>().InSingletonScope();

        Bind<ICommand>().To<HelpCommand>();
        Bind<ICommand>().To<NewCommand>();
        Bind<ICommand>().To<LastCommand>();
        Bind<ICommand>().To<RemoveCommand>();
    }
}