using Ninject;

namespace Bot.TelegramBot.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IKernel _kernel;
    private readonly Dictionary<string, Type> _commandMap;

    public CommandFactory(IKernel kernel, Dictionary<string, Type> commandMap)
    {
        _kernel = kernel;
        _commandMap = commandMap;
    }

    public ICommand CreateCommand(User user, string? message, CancellationToken cancellationToken)
    {
        if (user.State.EnteringQuery || user.State.ConfirmingQuery)
            return ExecuteCommand<NewCommand>();

        if (user.State.RemovingQuery || user.State.ConfirmingRemoval)
            return ExecuteCommand<RemoveCommand>();

        if (user.State.EnteringQueryToSeeLastArticles || user.State.EnteringMaxArticlesToSeeLast)
            return ExecuteCommand<LastCommand>();

        var command = _kernel.GetAll<ICommand>()
            .FirstOrDefault(cmd => cmd.Command.Equals(message, StringComparison.OrdinalIgnoreCase) ||
                                   cmd.Name.Equals(message, StringComparison.OrdinalIgnoreCase));

        return command ?? throw new InvalidOperationException("Unknown command");
    }

    private T ExecuteCommand<T>() where T : ICommand => _kernel.Get<T>();
}