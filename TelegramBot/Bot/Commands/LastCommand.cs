using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class LastCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly User _user;
    private readonly string _message;
    private readonly CancellationToken _cancellationToken;

    public LastCommand(ITelegramBotClient botClient, User user, string message, CancellationToken cancellationToken)
    {
        _botClient = botClient;
        _user = user;
        _message = message;
        _cancellationToken = cancellationToken;
    }
    
    public async Task Execute()
    {
        if (_user.State.EnteringQueryToSeeLastArticles)
            await SendLastArticles();
        else
            await SendUserQueries();
    }
    
    private async Task SendUserQueries()
    {
        var queries = _user.Queries;
        var queriesMessage = queries.Aggregate("", (current, query) => current + $"{query}\n");
        var buttons = queries
            .Select(query => new KeyboardButton(query.ToString()))
            .Append(new KeyboardButton("Отмена"));
        var keyboard = new ReplyKeyboardMarkup(buttons)
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };

        if (queriesMessage.Length == 0)
        {
            await _botClient.SendMessage(chatId: _user.Id, text: "У Вас нет запросов в рассылке",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.EnteringQueryToSeeLastArticles = true;
            MessageHandler.UpdateUserInDatabase(_user);
            await _botClient.SendMessage(chatId: _user.Id, 
                text: $"Выберите запрос для просмотра последних пяти новых статей:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: _cancellationToken);
        }
    }
    
    private async Task SendLastArticles()
    {
        _user.State.EnteringQueryToSeeLastArticles = false;
        var query = _user.Queries
            .FirstOrDefault(q => q.ToString().Equals(_message, StringComparison.CurrentCultureIgnoreCase));

        if (_message == "Отмена")
        {
            await _botClient.SendMessage(chatId:  _user.Id, text: "Просмотр статей отменен",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else if (query is null)
        {
            await  _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{_message}\" не найден",
                replyMarkup: MessageHandler.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            await LastArticlesGetter.SendLastArticles(_botClient, _user, query.Text, _cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(_user);
    }
}