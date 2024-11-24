using Ninject;
using Ninject.Parameters;

namespace Bot.TelegramBot.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IKernel _kernel;
    private User _user;
    private string? _message;
    private CancellationToken _cancellationToken;

    public CommandFactory(IKernel kernel)
    {
        _kernel = kernel;
    }

    public ICommand CreateCommand(User user, string? message, CancellationToken cancellationToken)
    {
        _user = user;
        _message = message;
        _cancellationToken = cancellationToken;
        
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
    
    private T ExecuteCommand<T>() where T : ICommand =>
        _kernel.Get<T>(new ConstructorArgument("user", _user),
            new ConstructorArgument("cancellationToken", _cancellationToken),
            new ConstructorArgument("message", _message));
}