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

        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        foreach (var commandType in commandTypes)
            Bind(typeof(ICommand)).To(commandType);
    }
}