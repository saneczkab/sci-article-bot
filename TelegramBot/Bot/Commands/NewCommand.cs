using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class NewCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly User _user;
    private readonly string _message;
    private readonly CancellationToken _cancellationToken;

    public NewCommand(ITelegramBotClient botClient, User user, string message, CancellationToken cancellationToken)
    {
        _botClient = botClient;
        _user = user;
        _message = message;
        _cancellationToken = cancellationToken;
    }

    public async Task Execute()
    {
        if (_user.State.EnteringQuery)
            await GetQueryText();
        else if (_user.State.ConfirmingQuery)
            await Confirm();
        else
        {
            _user.State.EnteringQuery = true;
            MessageHandler.UpdateUserInDatabase(_user);
            await _botClient.SendMessage(chatId: _user.Id, text: "Введите запрос для добавления в рассылку:", 
                cancellationToken: _cancellationToken);
        }
    }

    private async Task GetQueryText()
    {
        var query = new Query(_message);
        _user.State.EnteringQuery = false;
        
        if (_user.Queries.Contains(query))
        {
            await _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{query}\" уже есть в рассылке",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ConfirmingQuery = true;
            _user.State.ProcessingQuery = query;
            await LastArticlesGetter.SendLastArticles(_botClient, _user, _message, _cancellationToken);
            await _botClient.SendMessage(chatId: _user.Id, text: $"Вы хотите добавить запрос '{_message}' в рассылку?", 
                replyMarkup: MessageHandler.ConfirmationKeyboard, cancellationToken: _cancellationToken);
        }
        
        MessageHandler.UpdateUserInDatabase(_user);
    }

    private async Task Confirm()
    {
        if (_message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            _user.Queries.Add(_user.State.ProcessingQuery);
            _user.State.ConfirmingQuery = false;
            await _botClient.SendMessage(chatId: _user.Id, text: "Запрос добавлен в рассылку.", 
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ConfirmingQuery = false;
            await _botClient.SendMessage(chatId: _user.Id, text: "Запрос не был добавлен.", 
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        
        MessageHandler.UpdateUserInDatabase(_user);
    }
}