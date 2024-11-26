using Bot.Bot;
using Ninject;

namespace Bot.TelegramBot.Commands;

public class CommandFactory : ICommandFactory
{
    public ICommand CreateCommand(User user, string? message, CancellationToken cancellationToken)
    {
        if (user.State.EnteringQuery || user.State.ConfirmingQuery)
            return ExecuteCommand<NewCommand>();

        if (user.State.RemovingQuery || user.State.ConfirmingRemoval)
            return ExecuteCommand<RemoveCommand>();

        if (user.State.EnteringQueryToSeeLastArticles || user.State.EnteringMaxArticlesToSeeLast)
            return ExecuteCommand<LastCommand>();

        var command = KernelHandler.Kernel.GetAll<ICommand>()
            .FirstOrDefault(cmd => cmd.Command.Equals(message, StringComparison.OrdinalIgnoreCase) ||
                                   cmd.Name.Equals(message, StringComparison.OrdinalIgnoreCase));

        return command ?? ExecuteCommand<UnknownCommand>();
    }

    private static T ExecuteCommand<T>() where T : ICommand => KernelHandler.Kernel.Get<T>();
}