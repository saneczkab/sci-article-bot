using Bot.Models;
using Bot.TelegramBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.TelegramBot.Commands;

public class LastCommand : ICommand
{
    private ITelegramBotClient _botClient;
    private User _user;
    private CancellationToken _cancellationToken;
    private string _message;
    public string Command => "/last";
    public string Name => "Последние статьи";
    public string Description => "показать последние статьи по запросу";
    
    public async Task Execute(ITelegramBotClient botClient, User user, 
        CancellationToken cancellationToken, string message)
    {
        _botClient = botClient;
        _user = user;
        _cancellationToken = cancellationToken;
        _message = message;
        
        if (_user.State.EnteringQueryToSeeLastArticles)
            await GetArticlesAmount();
        else if (_user.State.EnteringMaxArticlesToSeeLast)
            await SendLastArticles();
        else
            await GetQuery();
    }
    
    private async Task GetQuery()
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

        _user.State.EnteringQueryToSeeLastArticles = true;
        MessageHandler.UpdateUserInDatabase(_user);
        await _botClient.SendMessage(chatId: _user.Id, 
            text: "Введите запрос для просмотра последних новых статей.",
            replyMarkup: keyboard, cancellationToken: _cancellationToken);
        
        if (queriesMessage.Length > 0)
            await _botClient.SendMessage(chatId: _user.Id, 
                text: $"Вы можете как ввести новый запрос, так и выбрать один из подключённых:\n{queriesMessage}",
                replyMarkup: keyboard, cancellationToken: _cancellationToken);
    }

    private async Task GetArticlesAmount()
    {
        _user.State.EnteringQueryToSeeLastArticles = false;
        
        if (_message == "Отмена")
        {
            await _botClient.SendMessage(chatId:  _user.Id, text: "Просмотр статей отменен",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else
        {
            _user.State.ProcessingQuery = new Query(_message);
            _user.State.EnteringMaxArticlesToSeeLast = true;
            await _botClient.SendMessage(chatId: _user.Id, 
                text: "Введите количество статей для просмотра (не более 25):",
                cancellationToken: _cancellationToken);
        }
        
        MessageHandler.UpdateUserInDatabase(_user);
    }
    
    private async Task SendLastArticles()
    {
        if (_message == "Отмена")
        {
            _user.State.EnteringMaxArticlesToSeeLast = false;
            _user.State.EnteringQueryToSeeLastArticles = false;
            await _botClient.SendMessage(chatId:  _user.Id, text: "Просмотр статей отменен",
                replyMarkup: Keyboards.CommandsKeyboard, cancellationToken: _cancellationToken);
        }
        else if (int.TryParse(_message, out var maxArticles) && maxArticles is > 0 and <= 25)
        {
            _user.State.EnteringMaxArticlesToSeeLast = false;
            await LastArticlesGetter.SendLastArticles(_botClient, _user, maxArticles, _cancellationToken);
        }
        else
        {
            await _botClient.SendMessage(chatId: _user.Id, text: "Введите корректное число статей (от 1 до 25):",
                cancellationToken: _cancellationToken);
        }

        MessageHandler.UpdateUserInDatabase(_user);
    }
}