using Ninject;
using Ninject.Parameters;

namespace Bot.TelegramBot.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IKernel _kernel;

    public CommandFactory(IKernel kernel)
    {
        _kernel = kernel;
    }

    public ICommand CreateCommand(User user, string? message, CancellationToken cancellationToken)
    {
        if (user.State.EnteringQuery || user.State.ConfirmingQuery)
            return ExecuteCommand<NewCommand>();

        if (user.State.RemovingQuery || user.State.ConfirmingRemoval)
            return ExecuteCommand<RemoveCommand>();

        if (user.State.EnteringQueryToSeeLastArticles)
            return ExecuteCommand<LastCommand>();

        return message switch
        {
            "/start" => ExecuteCommand<HelpCommand>(),
            "/help" => ExecuteCommand<HelpCommand>(),
            "/new" => ExecuteCommand<NewCommand>(),
            "/last" => ExecuteCommand<LastCommand>(),
            "/remove" => ExecuteCommand<RemoveCommand>(),
            _ => throw new InvalidOperationException("Unknown command")
        };
    }

    private T ExecuteCommand<T>() where T : ICommand
    {
        return _kernel.Get<T>();
    }
}