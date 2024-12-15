using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class RemoveCommand : ICommand
{
    private ITelegramBotClient _botClient;
    private User _user;
    private string _message;
    private CancellationToken _cancellationToken;
    private IKeyboards Keyboards { get; }
    public string Command => "/remove";
    public string Name => "Удалить запрос";
    public string Description => "удалить запрос из рассылки";

    public RemoveCommand(IKeyboards keyboards)
    {
        Keyboards = keyboards;
    }
    
    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        _botClient = botClient;
        _user = user;
        _cancellationToken = cancellationToken;
        _message = message;

        if (_user.State.RemovingQuery)
            await GetQueryText();
        else if (_user.State.ConfirmingRemoval)
            await Confirm();
        else
            await SendUserQueries();
    }

    private async Task GetQueryText()
    {
        _user.State.RemovingQuery = false;
        var query = _user.Queries
            .FirstOrDefault(q => q.ToString().Equals(_message, StringComparison.CurrentCultureIgnoreCase));

        if (_message == "Отмена")
        {
            await _botClient.SendMessage(chatId: _user.Id, text: "Удаление запроса отменено",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else if (query is null)
        {
            await _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{_message}\" не найден",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ConfirmingRemoval = true;
            _user.State.ProcessingQuery = query;
            await _botClient.SendMessage(chatId: _user.Id, text: $"Удалить запрос \"{query}\"?",
                replyMarkup: Keyboards.ConfirmationKeyboard, cancellationToken: _cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(_user);
    }

    private async Task Confirm()
    {
        _user.State.ConfirmingRemoval = false;
        var query = _user.State.ProcessingQuery;

        if (_message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            _user.Queries.Remove(query!);
            DatabaseConnection.UpdateUserInDatabase(_user);
            await _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{query}\" удален",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            await _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{query}\" не будет удален",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
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
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.RemovingQuery = true;
            DatabaseConnection.UpdateUserInDatabase(_user);
            await _botClient.SendMessage(chatId: _user.Id, text: $"Выберите запрос для удаления:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: _cancellationToken);
        }
    }
}