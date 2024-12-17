using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Ninject;

namespace Bot.TelegramBot.Commands;

public class CommandFactory : ICommandFactory
{
    private IKernel Kernel { get; }
    private IEnumerable<ICommand> Commands { get; }

    public CommandFactory(IKernel kernel, IEnumerable<ICommand> commands)
    {
        Kernel = kernel;
        Commands = commands;
    }
    
    public ICommand? GetCommand(User user, string? message, CancellationToken cancellationToken)
    {
        if (user.State.EnteringQuery || user.State.ConfirmingQuery)
            return Kernel.Get<NewCommand>();

        if (user.State.RemovingQuery || user.State.ConfirmingRemoval)
            return Kernel.Get<RemoveCommand>();

        // if (user.State.EnteringQueryToSeeLastArticles || user.State.EnteringMaxArticlesToSeeLast)
        //     return Kernel.Get<LastCommand>();
        
        var command = Commands
            .FirstOrDefault(cmd => cmd.Command.Equals(message, StringComparison.OrdinalIgnoreCase) ||
                                   cmd.Name.Equals(message, StringComparison.OrdinalIgnoreCase));

        return command;
    }
}