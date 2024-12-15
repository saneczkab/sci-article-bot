using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;

namespace Bot.TelegramBot.Commands;

public class NewCommand : ICommand
{
    private ITelegramBotClient _botClient;
    private User _user;
    private string _message;
    private CancellationToken _cancellationToken;
    public string Command => "/new";
    public string Name => "Новый запрос";
    public string Description => "добавить запрос в рассылку";

    public async Task Execute(ITelegramBotClient botClient, User user,
        CancellationToken cancellationToken, string message)
    {
        _botClient = botClient;
        _user = user;
        _cancellationToken = cancellationToken;
        _message = message;

        if (_user.Queries.Count >= Bot.MaxQueries)
            await _botClient.SendMessage(chatId: _user.Id,
                text: $"Вы не можете добавить больше {Bot.MaxQueries} запросов в рассылку.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        else if (_user.State.EnteringQuery)
            await GetQueryText();
        else if (_user.State.ConfirmingQuery)
            await Confirm();
        else
        {
            _user.State.EnteringQuery = true;
            DatabaseConnection.UpdateUserInDatabase(_user);
            await _botClient.SendMessage(chatId: _user.Id, text: "Введите запрос для добавления в рассылку:",
                cancellationToken: _cancellationToken);
        }
    }

    private async Task GetQueryText()
    {
        var query = new Query(char.ToUpper(_message[0]) + _message[1..].ToLower());
        _user.State.EnteringQuery = false;

        if (_user.Queries.Contains(query))
        {
            await _botClient.SendMessage(chatId: _user.Id, text: $"Запрос \"{query}\" уже есть в рассылке",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else if (_message.Equals("отмена", StringComparison.CurrentCultureIgnoreCase))
        {
            await _botClient.SendMessage(chatId: _user.Id,
                text: $"Добавление запроса \"{query}\" по техническим причинам невозможно.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ConfirmingQuery = true;
            _user.State.ProcessingQuery = query;
            await LastArticlesGetter.SendLastArticles(_botClient, _user, 5, _cancellationToken);
            await _botClient.SendMessage(chatId: _user.Id, text: $"Вы хотите добавить запрос '{query}' в рассылку?",
                replyMarkup: Keyboards.ConfirmationKeyboard, cancellationToken: _cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(_user);
    }

    private async Task Confirm()
    {
        if (_message.Equals("да", StringComparison.CurrentCultureIgnoreCase))
        {
            _user.Queries.Add(_user.State.ProcessingQuery);
            _user.State.ConfirmingQuery = false;
            await _botClient.SendMessage(chatId: _user.Id, text: "Запрос добавлен в рассылку.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ConfirmingQuery = false;
            await _botClient.SendMessage(chatId: _user.Id, text: "Запрос не был добавлен.",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }

        DatabaseConnection.UpdateUserInDatabase(_user);
    }
}